using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using bookStore_API.Contracts;
using bookStore_API.Data;
using bookStore_API.DTOs;
using bookStore_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace bookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the authors in the bookstore's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IloggerService _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorRepository,
            IloggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Get all Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted Get All Authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Succesfully got all Authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{ e.Message} - { e.InnerException }");
            }

        }
        /// <summary>
        ///  Get An Author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {

            try
            {
                _logger.LogInfo("Attempted to Get Author with id:");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"author with id:{id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                return Ok(response);
            }
            catch (Exception e)
            {

                return InternalError($"{ e.Message} - { e.InnerException }");
            }
        }




        /// <summary>
        /// Create an Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author Submission Attempted");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"Empty request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Author Data was incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSucces = await _authorRepository.Create(author);
                if (!isSucces)
                {

                    return InternalError($"Author Creation Failed");
                }
                _logger.LogInfo("Author Created Succesfully");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {

                return InternalError($"{ e.Message} - { e.InnerException }");
            }
        }
        /// <summary>
        /// Update Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPut("{id}")]  
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author Updated Attempted - id: {id}");
                if(id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"Author update Failed with bad Data");
                    return BadRequest();
                }
                var isExist = await _authorRepository.isExists(id);
                if (!isExist)
                {
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Author data is incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSucces = await _authorRepository.Update(author);
                if (!isSucces)
                {
                   return InternalError($"Update Operation failed");
                }
                _logger.LogWarn($"Author with id: {id} is succesfully updated");
                return NoContent();
            }
            catch (Exception e)
            {

                return InternalError($"{ e.Message} - { e.InnerException }");
            }
        }
        /// <summary>
        /// Update Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Author with the id {id} delete Attempted");
                if(id < 1)
                {
                    _logger.LogWarn($"Author delete failed with bad data");
                    return BadRequest();
                }
                var isExist = await _authorRepository.isExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"Author with id :{id} was not found");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(id);
                var isSucces = await _authorRepository.Delete(author);
                if (!isSucces)
                {
                    return InternalError($"Author Delete Failed ");
                }
                _logger.LogWarn($"Author with the id {id} is succesfully deleted");
                return NoContent();
            }
            catch (Exception e)
            {

                return InternalError($"{ e.Message} - { e.InnerException }");
            }
        }

            private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the administrator");
        }

    }
}
