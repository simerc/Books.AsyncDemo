using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Filters;
using Books.Api.Models;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [Route("api/bookcollections")]
    [ApiController]
    [BooksResultFilter] //used at controller level as both actions result in a BooksResult
    public class BookCollectionsController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BookCollectionsController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        //Accept a comma separated list of guids, these can then be bound as the parameter of IEnumerable
        //ModelBinder is used
        //
        // api/bookscollection/({id1},{id2},{id3})......
        [HttpGet("({bookids})", Name="GetBookCollection")]
        public async Task<IActionResult> GetBookCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> bookIds)
        {
            var books = await _booksRepository.GetBooksAsync(bookIds);

            if (books.Count() != bookIds.Count())
            {
                return NotFound();
            }

            return Ok(books);
        }

        public async Task<IActionResult> CreateBookCollection(
            [FromBody] IEnumerable<BookForCreation> bookCollection)
        {
            var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);

            foreach (var book in bookEntities)
            {
                _booksRepository.AddBook(book);
            }

            await _booksRepository.SaveChangesAsync();

            var booksToReturn = await _booksRepository.GetBooksAsync(bookEntities.Select(b => b.Id).ToList());

            var bookIds = string.Join(",", booksToReturn.Select(x => x.Id));

            return CreatedAtRoute("GetBookCollection", 
                new {bookIds}, 
                booksToReturn);
        }
    }
}
