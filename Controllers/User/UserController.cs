using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynergyCore.user;
using SynergyEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OASystemSynergy.Controllers.User
{
    [EnableCors("any")]
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserFactory<tb_User> _userFactory;
        public UserController(UserFactory<tb_User> userFactory)
        {
            _userFactory = userFactory;
        }
        [Route("Login")]
        [HttpGet]
        public int Login(string UserName, string PassWord)
        {
            int a = _userFactory.Login(UserName, PassWord);
            if (a == 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
