using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.ExternalModels;
using Books.Api.Filters;
using Books.Api.Models;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BooksController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [BooksResultFilter]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksRepository.GetBooksAsync();

            return Ok(books);
        }

        [HttpGet]
        [Route("{id}", Name="GetBook")]
        [BookWithCoversResultFilter]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);

            if (book == null)
                return NotFound();

            var bookCovers = await _booksRepository.GetBookCoversAsync(id);


            // pre csharp 7
            //var propertyBag = new Tuple<Entities.Book, IEnumerable<ExternalModels.BookCover>>
            //    (book, bookCovers);

            //(Entities.Book book, IEnumerable<ExternalModels.BookCover> bookCovers) 
            //    propertyBag = (book, bookCovers);

            return Ok((book, bookCovers));
        }

        [HttpPost]
        [BookResultFilter]
        public async Task<IActionResult> CreateBook([FromBody] BookForCreation book)
        {
            var bookEntity = _mapper.Map<Entities.Book>(book);

            _booksRepository.AddBook(bookEntity);

            await _booksRepository.SaveChangesAsync();

            //re-fetch book
            await _booksRepository.GetBookAsync(bookEntity.Id);

            return CreatedAtRoute("GetBook", 
                new {id = bookEntity.Id}, 
                bookEntity);
        }
    }
}
