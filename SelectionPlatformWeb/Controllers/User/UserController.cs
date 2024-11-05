using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SelectionPlatform.Auth;
using SelectionPlatform.IRepository;
using SelectionPlatform.IServices.User;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Services.User;
using SelectionPlatform.Utility.Extensions;
using System.Text.Json;

namespace SelectionPlatformWeb.Controllers.User
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;
        private readonly IJwtServices _jwtServices;
        public UserController(IRepositoryWrapper repositoryWrapper, ILogger<UserController> logger, IUserServices userServices, IMapper mapper, IJwtServices jwt)
        {
            _userServices = userServices;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
            _jwtServices = jwt;
        }


        [HttpPost]
        public IActionResult Login([FromForm] UserLogin user)
        {
            if (ModelState.IsValid)
            {
                var userlog = _userServices.GetUsers(new UserQueryParameters { Account = user.Account }).FirstOrDefault();
                if (userlog != null)
                {
                    return Ok(_jwtServices.GenerateToken(user.Account, userlog.Email));
                }
                else { return BadRequest(); }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        //[Authorize]
        public IActionResult GetUsers([FromQuery] UserQueryParameters userQueryParameters)
        {
            try
            {
                var vv = User.FindFirst("userId")?.Value;
                if (!userQueryParameters.ValidDataRange)
                {
                    return BadRequest("日期范围错误");
                }
                var users = _userServices.GetUsers(userQueryParameters);
                if (users == null)
                {
                    return NotFound();
                }
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(users.MetaData));
                var usersdto = _mapper.Map<IEnumerable<UserDto>>(users).ShapeData(userQueryParameters.Fields);
                return Ok(usersdto);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("{account}", Name = "adduser")]
        public IActionResult GetUserByAccount(string account)
        {
            try
            {
                var users = _userServices.GetUserByAccount(account);
                if (users == null)
                {
                    return NotFound();
                }
                var userdtos = _mapper.Map<UserDto>(users);
                return Ok(userdtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        //[HttpGet]
        //public IActionResult GetTime(string iii)
        //{
        //    return Ok();
        //}

        //[HttpGet("get/{vv}")]
        //public IActionResult GetTime2(string vv)
        //{
        //    return Ok();
        //}




        [HttpPost]
        public IActionResult AddUser(CreateUserDto user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("数据无效");
                }
                var _userentity = _mapper.Map<UserEntity>(user);
                _userServices.Insert(_userentity);

                var createuser = _mapper.Map<UserDto>(_userentity);
                return CreatedAtRoute("adduser", new { account = createuser.Account }, createuser);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        [HttpPut("{account}")]
        [Authorize(Policy = "User")]
        public IActionResult UpdateUser(string account, UpdateUserDto updateUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("数据无效");
            }

            var userentity = _userServices.GetUserByAccount(account);
            if (userentity == null)
            {
                return NotFound("用户不存在");
            }
            userentity.UpdateTime = DateTime.Now;
            _mapper.Map(updateUser, userentity);
            _userServices.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{account}")]
        public IActionResult DeleteUser(string account)
        {

            var userentity = _userServices.GetUserByAccount(account);
            if (userentity == null)
            {
                return NotFound("用户不存在");
            }
            _userServices.Delete(userentity);
            _userServices.SaveChangesAsync();

            return NoContent();
        }
    }
}
