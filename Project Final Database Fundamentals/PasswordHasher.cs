using System;
using System.Collections.Generic;
using System.Text;

namespace Project_Final_Database_Fundamentals
{
    internal class PasswordHasher
    {
        // Método para crear un hash (para el registro)
        public static string HashPassword(string password)
        {
            // BCrypt genera automáticamente una "salt" y la incluye en el hash final
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Método para verificar un hash (para el login)
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // BCrypt compara el password ingresado con el hash almacenado
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
