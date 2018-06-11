using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using LearnWithMentorDAL.Entities;
using LearnWithMentorDAL.UnitOfWork;
using LearnWithMentorDTO;

namespace LearnWithMentor.Controllers
{
    /// <summary>
    /// Controller for working with tasks
    /// </summary>
    public class TaskController : ApiController
    {
        private IUnitOfWork UoW;
        
        /// <summary>Initialise local unit of work.</summary>
        public TaskController()
        {
            UoW = new UnitOfWork(new LearnWithMentor_DBEntities());
        }
        /// <summary>
        /// Returns a list of all tasks.
        /// </summary>
        // GET api/task      
        [HttpGet]
        [Route("api/task")]
        public IEnumerable<TaskDTO> Get()
        {
            List<TaskDTO> dto = new List<TaskDTO>();
            foreach (var t in UoW.Tasks.GetAll())
            {
                dto.Add(new TaskDTO(t.Id,
                                    t.Name,
                                    t.Description,
                                    t.Private,
                                    t.Create_Id,
                                    UoW.Users.ExtractFullName(t.Create_Id),
                                    t.Mod_Id,
                                    UoW.Users.ExtractFullName(t.Mod_Id),
                                    t.Create_Date,
                                    t.Mod_Date,
                                    null,
                                    null));
            }
            if (dto == null) return null;
            return dto;
        }
        /// <summary>
        /// Returns task by ID.
        /// </summary>
        // GET api/task/5
        [HttpGet]
        [Route("api/task/{id}")]
        public TaskDTO Get(int id)
        {
            Task t = UoW.Tasks.Get(id);
            if (t == null) return null;
            return new TaskDTO(t.Id,
                               t.Name,
                               t.Description,
                               t.Private,
                               t.Create_Id,
                               UoW.Users.ExtractFullName(t.Create_Id),
                               t.Mod_Id,
                               UoW.Users.ExtractFullName(t.Mod_Id),
                               t.Create_Date,
                               t.Mod_Date,
                               null,
                               null);
        }
        /// <summary>
        /// Returns tasks with priority and section for defined by ID plan.
        /// </summary>
        /// <param name="id">ID of the tast.</param>
        /// <param name="planId">ID of the plan.</param>
        // GET api/task/search?id={id}&planid={planid}
        [HttpGet]
        [Route("api/task")]
        public TaskDTO Get(int id,int planId )
        {
            Task t = UoW.Tasks.Get(id);
            if (t == null || !UoW.PlanTasks.ContainsTaskInPlan(id,planId)) return null;
            return new TaskDTO(t.Id,
                               t.Name,
                               t.Description,
                               t.Private,
                               t.Create_Id,
                               UoW.Users.ExtractFullName(t.Create_Id),
                               t.Mod_Id,
                               UoW.Users.ExtractFullName(t.Mod_Id),
                               t.Create_Date,
                               t.Mod_Date,
                               t.PlanTasks.Where(pt => pt.Task_Id == t.Id && pt.Plan_Id == planId).FirstOrDefault().Priority,
                               t.PlanTasks.Where(pt => pt.Task_Id == t.Id && pt.Plan_Id == planId).FirstOrDefault().Section_Id);
        }
        /// <summary>
        /// Returns tasks which name contains special string key, searches only in plan if planId given.
        /// </summary>
        /// <param name="key">Key for search.</param>
        /// <param name="planId">ID of the plan.</param>
        // GET api/task/search?key={key}&planid={planid}
        [HttpGet]
        [Route("api/task/search")]
        public IEnumerable<TaskDTO> Search(string key, int? planId)
        {
            if (key == null)
            {
                return Get();
            }
            string[] lines = key.Split(' ');
            List<TaskDTO> dto = new List<TaskDTO>();
            foreach (var t in UoW.Tasks.Search(lines, planId))
            {
                dto.Add(new TaskDTO(t.Id,
                                    t.Name,
                                    t.Description,
                                    t.Private,
                                    t.Create_Id,
                                    UoW.Users.ExtractFullName(t.Create_Id),
                                    t.Mod_Id,
                                    UoW.Users.ExtractFullName(t.Mod_Id),
                                    t.Create_Date,
                                    t.Mod_Date,
                                    t.PlanTasks.Where(pt => pt.Task_Id == t.Id && pt.Plan_Id == planId).FirstOrDefault()?.Priority,
                                    t.PlanTasks.Where(pt => pt.Task_Id == t.Id && pt.Plan_Id == planId).FirstOrDefault()?.Section_Id));
            }
            return dto;
        }
        // POST api/task
        /// <summary>
        /// Creates new task
        /// </summary>
        /// <param name="t">Task object for creation.</param>
        [HttpPost]
        [Route("api/task")]
        public IHttpActionResult Post([FromBody]TaskDTO t)
        {
            UoW.Tasks.Add(t);
            UoW.Save();
            return Ok();
        }

        // PUT api/task/5
        /// <summary>
        /// Updates task by ID
        /// </summary>
        /// <param name="id">Task ID for update.</param>
        /// <param name="t">Modified task object for update.</param>
        [HttpPut]
        [Route("api/task/{id}")]
        public IHttpActionResult Put(int id, [FromBody]TaskDTO t)
        {
            UoW.Tasks.UpdateById(id,t);
            UoW.Save();
            return Ok();
        }

        // DELETE api/task/5
        /// <summary>
        /// Deletes task by ID
        /// </summary>
        /// <param name="id">Task ID for delete.</param>
        [HttpDelete]
        [Route("api/task/{id}")]
        public IHttpActionResult Delete(int id)
        {
            UoW.Tasks.RemoveById(id);
            UoW.Save();
            return Ok();
        }

        /// <summary>
        /// Returns a list of comments for defined by ID task.
        /// </summary>
        /// <param name="taskId">Task ID.</param>
        [Route("api/task/{taskId}/comment")]
        public IEnumerable<CommentDTO> GetComments(int taskId)
        {
            var comments = UoW.Comments.GetAll().Where(c => c.PlanTask_Id == taskId);
            if (comments == null) return null;
            List<CommentDTO> dto = new List<CommentDTO>();
            foreach (var a in comments)
            {
                dto.Add(new CommentDTO(a.Id, a.Text, a.Create_Id, a.Users.FirstName, a.Users.LastName, a.Create_Date, a.Mod_Date));
            }
            return dto;
        }

        /// <summary>
        /// Creates comment for defined by ID task.
        /// </summary>
        /// <param name="value">Comment object for creation.</param>
        /// <param name="taskId">Task ID.</param>
        [HttpPost]
        [Route("api/task/{taskId}/comment")]
        public IHttpActionResult AddComment([FromBody]CommentDTO value, int taskId)
        {
            UoW.Comments.Add(value, taskId);
            UoW.Save();
            return Ok();
        }
    }
}
