using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Books.Api.Context;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Books.Api.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _booksContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BooksRepository> _logger;

        private CancellationTokenSource _cancellationTokenSource;

        public BooksRepository(BooksContext booksContext, IHttpClientFactory httpClientFactory, ILogger<BooksRepository> logger)
        {
            _booksContext = booksContext ?? throw new ArgumentNullException(nameof(booksContext));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            //simulate network delay
            _booksContext.Database.ExecuteSqlCommand("WAITFOR DELAY '00:00:02';");

            return await _booksContext.Books.Include(a => a.Author).
                                                ToListAsync();
        }

        public async Task<Book> GetBookAsync(Guid id)
        {
            //simulate network delay
            _booksContext.Database.ExecuteSqlCommand("WAITFOR DELAY '00:00:02';");

            return await _booksContext.Books.Include(a => a.Author).
                                                FirstOrDefaultAsync(x => x.Id == id);
        }

        public IEnumerable<Book> GetBooks()
        {
            //simulate network delay
            _booksContext.Database.ExecuteSqlCommand("WAITFOR DELAY '00:00:02';");

            return _booksContext.Books.Include(a => a.Author).ToList();
        }

        public async Task<IEnumerable<Entities.Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            var books = await _booksContext.Books.Where(b => bookIds.Contains(b.Id))
                .Include(a => a.Author)
                .ToListAsync();

            return books;
        }

        public void AddBook(Entities.Book bookToAdd)
        {
            if (bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _booksContext.Add(bookToAdd);
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync($"https://localhost:44339/api/bookcovers/{coverId}");

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<BookCover>(
                    await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookid)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();

            _cancellationTokenSource = new CancellationTokenSource();

            var bookCoverUrls = new[]
            {
                $"https://localhost:44339/api/bookcovers/{bookid}-dummycover1",
                $"https://localhost:44339/api/bookcovers/{bookid}-dummycover2",
                $"https://localhost:44339/api/bookcovers/{bookid}-dummycover3",
                $"https://localhost:44339/api/bookcovers/{bookid}-dummycover4",
                $"https://localhost:44339/api/bookcovers/{bookid}-dummycover5"
            };

            //foreach (var bookCoverUrl in bookCoverUrls)
            //{
                
            //}

            //create the task list using Linq
            var downloadBookCoversTaskQuery =
                from bookCoverUrl
                    in bookCoverUrls
                select DownloadBookCoverAsync(httpClient, bookCoverUrl, _cancellationTokenSource.Token);

            //run the tasks
            var downloadBookCoverTasks = downloadBookCoversTaskQuery.ToList();

            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                _logger.LogInformation($"{operationCanceledException.Message}");
                foreach (var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return new List<BookCover>();
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message}");
                throw;
            }
            
        }

        private async Task<BookCover> DownloadBookCoverAsync(
            HttpClient client, string bookCoverUrl, CancellationToken cancelToken)
        {
            //throw new Exception("Author is writing the book far too slowly");

            var response = await client.GetAsync(bookCoverUrl, cancelToken);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonConvert.DeserializeObject<BookCover>(
                    await response.Content.ReadAsStringAsync());

                return bookCover;
            }

            _cancellationTokenSource.Cancel();

            return null;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _booksContext.SaveChangesAsync() > 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_booksContext != null)
                {
                    _booksContext.Dispose();
                    _booksContext = null;
                }

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }
    }
}
