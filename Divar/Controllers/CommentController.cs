﻿namespace Divar.Controllers
{
    public class CommentController : Controller
    {
        private readonly string _connectionString;

        public CommentController()
        {
            _connectionString = "Server=.; Initial Catalog=Divar; Integrated Security=True; encrypt=False";
        }


        // Create comment
        [HttpGet]
        [Authorize(Policy = "RequireCommentCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "RequireCommentCreate")]
        public async Task<IActionResult> Create(Comment comment)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("sp_InsertComment", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Body", comment.Body);
                    command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    await command.ExecuteNonQueryAsync();
                }
                return RedirectToAction("Index");
            }
            return View(comment);
        }



        // Comment list
        [Authorize(Policy = "RequireCommentIndex")]
        public async Task<IActionResult> Index()
        {
            var comments = new List<Comment>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT Id, Body, CreatedDate FROM Comments", connection);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    comments.Add(new Comment
                    {
                        Id = reader.GetInt32(0),
                        Body = reader.GetString(1),
                        CreatedDate = reader.GetDateTime(2)
                    });
                }
            }

            return View(comments);
        }




        // Edit comment
        [HttpGet]
        [Authorize(Policy = "RequireCommentEdit")]
        public async Task<IActionResult> Edit(int id)
        {
            Comment comment = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT Id, Body, CreatedDate FROM Comments WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    comment = new Comment
                    {
                        Id = reader.GetInt32(0),
                        Body = reader.GetString(1),
                        CreatedDate = reader.GetDateTime(2)
                    };
                }
            }

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }


        [HttpPost]
        [Authorize(Policy = "RequireCommentEdit")]
        public async Task<IActionResult> Edit(Comment comment)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("sp_UpdateComment", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", comment.Id);
                    command.Parameters.AddWithValue("@Body", comment.Body);
                    command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now); 

                    await command.ExecuteNonQueryAsync();
                }
                return RedirectToAction("Index");
            }
            return View(comment);
        }
    }
}
