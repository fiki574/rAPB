﻿using System.Text;
using System.Security.Cryptography;

namespace LobbyServer
{
    public class Hash
    {
        public static string SHA(string text)
        {
            SHA256 sha = new SHA256CryptoServiceProvider();
            sha.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            byte[] result = sha.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++) strBuilder.Append(result[i].ToString("x2"));
            return strBuilder.ToString();
        }
    }
}