namespace Divar.Models
{
    public class AdoAdminRepository : IAdminRepository
    {
        private readonly string _connectionString;

        public AdoAdminRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<CustomUser>> GetUsersAsync()
        {
            var users = new List<CustomUser>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetUsers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new CustomUser
                        {
                            Id = reader["Id"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString()
                        });
                    }
                }
            }
            return users;
        }



        public async Task<List<Advertisement>> GetAdvertisementsAsync()
        {
            var advertisements = new List<Advertisement>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetAdvertisements", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var ad = new Advertisement
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Title = reader["Title"].ToString(),
                            Content = reader["Content"].ToString(),
                            ImageUrl = reader["ImageUrl"].ToString(),
                            Price = Convert.ToInt32(reader["Price"]),
                            CreatedDate = reader["CreatedDate"] as DateTime?,
                            Category = (CategoryType)Enum.Parse(typeof(CategoryType), reader["Category"].ToString()),
                            CustomUserId = reader["CustomUserId"].ToString(),
                            CustomUser = new CustomUser
                            {
                                FirstName = reader["UserFirstName"]?.ToString(),
                                LastName = reader["UserLastName"]?.ToString()
                            },
                            MobileBrand = reader["MobileBrand"]?.ToString(),
                            SimCardsNumber = reader["SimCardsNumber"] != DBNull.Value ? Convert.ToInt32(reader["SimCardsNumber"]) : 0,
                            HomeSize = reader["HomeSize"] != DBNull.Value ? Convert.ToInt32(reader["HomeSize"]) : (int?)null,
                            HomeAddress = reader["HomeAddress"]?.ToString(),
                            BookAuthor = reader["BookAuthor"]?.ToString(),
                            CarBrand = reader["CarBrand"]?.ToString(), // واکشی مقدار CarBrand
                            GearboxType = reader["GearboxType"] as bool?
                        };
                        advertisements.Add(ad);
                    }
                }
            }
            return advertisements;
        }


        public async Task<List<Comment>> GetCommentsAsync()
        {
            var comments = new List<Comment>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetComments", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        comments.Add(new Comment
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Body = reader["Body"].ToString(),
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                        });
                    }
                }
            }
            return comments;
        }

        public async Task<CustomUser> GetUserByIdAsync(string id)
        {
            CustomUser user = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetUserById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Id", id);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = new CustomUser
                        {
                            Id = reader["Id"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString()
                        };
                    }
                }
            }
            return user;
        }


        public async Task DeleteUserAsync(string id, HttpContext httpContext)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DeleteUserAndAdvertisements", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Id", id);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                // پاک کردن سشن فقط برای کاربر حذف شده
                if (httpContext.Session.GetString("UserId") == id)
                {
                    httpContext.Session.Clear();
                }
            }
        }


        // for delete comments
        public async Task DeleteComment(int id, HttpContext httpContext)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DeleteComment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Id", id);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
