using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class AnswerController:BaseApiController
    {
       

        #region Constructor
        public AnswerController(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) : base(context, roleManager, userManager, configuration)
        {
        }
        #endregion
        #region RESTFUL conventions methods

        /// <summary>
        /// GET: api/answer/{}id
        /// Retrieves the Quiz with the given {id}
        /// </summary>
        /// <param name="id">The ID of an existing Quiz</param>
        /// <returns>the Quiz with the given {id}</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var answer = DbContext.Answers.Where(i => i.Id == id).FirstOrDefault();

            // handle requests asking for non-existing answers
            if (answer == null)
            {
                return NotFound(new
                {
                    Error = string.Format("Answer ID {0} has not been found", id)
                });
            }

            return new JsonResult(
                answer.Adapt<AnswerViewModel>(),
                JsonSettings);
        }

        /// <summary>
        /// Adds a new Quiz to the Database
        /// </summary>
        /// <param name="m">The AnswerViewModel containing the data to insert</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]AnswerViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid
            if (model == null) return new StatusCodeResult(500);

            // map the ViewModel to the Model
            var answer = model.Adapt<Answer>();

            // override those properties
            // that should be set from the server-side only
            answer.QuestionId = model.QuestionId;
            answer.Text = model.Text;
            answer.Notes = model.Notes;

            // properties set from server-side
            answer.CreatedDate = DateTime.Now;
            answer.LastModifiedDate = answer.CreatedDate;

            // add the new answer
            DbContext.Answers.Add(answer);
            // persist the changes into the Database
            DbContext.SaveChanges();

            // return the newly-created Ansewr to the client.
            return new JsonResult(answer.Adapt<AnswerViewModel>(), JsonSettings);
        }

        /// <summary>
        /// Edit the Answer with the given {id}
        /// </summary>
        /// <param name="m">The AnswerViewModel containing the data to update</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]AnswerViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid
            if (model == null) return new StatusCodeResult(500);

            // retrieve the answer to edit
            var answer = DbContext.Answers.Where(q => q.Id == model.Id).FirstOrDefault();

            // handle requests asking for non-existing answers
            if (answer == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Answer ID {0} has not been found", model.Id)
                });
            }

            // handle the update (without object-mapping)
            // by manually assigning the properties
            // we want to accept from the request
            answer.QuestionId = model.QuestionId;
            answer.Text = model.Text;
            answer.Value = model.Value;
            answer.Notes = model.Notes;

            // properties set from server-side
            answer.LastModifiedDate = answer.CreatedDate;

            // persist the changes into the Database
            DbContext.SaveChanges();

            // return the updated Quiz to the client.
            return new JsonResult(answer.Adapt<AnswerViewModel>(),
               JsonSettings);
        }


        /// <summary>
        /// Deletes the quiz with the given {id} from the Database
        /// </summary>
        /// <param name="id">The ID of an existing Answer</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // retrieve the answer from the Database
            var answer = DbContext.Answers.Where(i => i.Id == id)
                .FirstOrDefault();

            // handle requests asking for non-existing answers
            if (answer == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Answer ID {0} has not been found", id)
                });
            }

            // remove the quiz from the DbContext
            DbContext.Answers.Remove(answer);
            // persist the changes into the Database.
            DbContext.SaveChanges();

            // return an HTTP Status 200 (OK)
            return new OkResult();
        }
        #endregion


        #region Attribute-based routing methods
        // GET api/answer/all
        [HttpGet("All/{questionId}")]
        public IActionResult All(int questionId)
        {
            var answers = DbContext.Answers
                .Where(q => q.QuestionId == questionId)
                .ToArray();
            return new JsonResult(
                answers.Adapt<AnswerViewModel[]>(),
                JsonSettings);
        }

        #endregion
    }
}
