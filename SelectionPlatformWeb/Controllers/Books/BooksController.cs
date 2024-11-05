using BookStoreApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Core.Operations;
using SelectionPlatform.Models.Models;

namespace SelectionPlatformWeb.Controllers.Books
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class BooksController : ControllerBase
    //{
    //    private readonly BooksService _booksService;
    //    public BooksController(BooksService booksService)
    //    {
    //        _booksService = booksService;
    //    }

    //    [HttpGet]
    //    public async Task<IActionResult> GetBooks()
    //    {
    //        var result = await _booksService.GetAsync();
    //        return Ok(result);
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> Addbooks(Book books)
    //    {
    //         await _booksService.CreateAsync(books);
    //        return Ok();
    //    }
    //}
}
