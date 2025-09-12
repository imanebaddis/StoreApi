namespace StoreApi.Infra.DTOs {
            public class UserUpdateDto {
         public string Email { get; set; }
         public string FirstName { get; set; }
         public string LastName { get; set; }
         public bool IsActive { get; set; }
     }

     public class LoginDto {
         public string Username { get; set; }
         public string Password { get; set; }
     }

     public class LoginResponseDto {
         public int UserId { get; set; }
         public string Username { get; set; }
         public string Email { get; set; }
         public string FirstName { get; set; }
         public string LastName { get; set; }
         public bool IsActive { get; set; }
         public string Message { get; set; }
     }
}