using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SynergyCommon.Context;
using SynergyCore.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OASystemSynergy.Controllers.User
{
   
    [Route("api/[controller]")]
    [ApiController]
    
    public class UsersController : ControllerBase
    {
        private readonly SqlDbContext _myContext;
        private readonly UserFactory _userFactory;
        public UsersController(SqlDbContext sqlDbContext, UserFactory userFactory)
        {
            _myContext = sqlDbContext;
            _userFactory = userFactory;
        }
        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UsersController>/5
        [HttpGet("Login")]   
        public int Get(string UserName, string PassWord)
        {

            int flag = 0;
            try
            {
                if (_userFactory.Login(UserName, PassWord) > 0)
                {       
                    flag = 1;   
                }
                else
                {
                    return flag;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally { }
            return flag;
        }

        // POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
