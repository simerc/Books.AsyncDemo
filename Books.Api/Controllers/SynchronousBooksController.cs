using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.Api.Context;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [ApiController]
    [Route("api/synchronousbooks")]
    public class SynchronousBooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;

        public SynchronousBooksController(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            var books = _booksRepository.GetBooks();

            return Ok(books);
        }
    }
}
