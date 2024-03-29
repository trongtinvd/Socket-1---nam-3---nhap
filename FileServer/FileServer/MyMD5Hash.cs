﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileServer
{
    static class MyMD5Hash
    {
        private static object hashLocker = new object();
        private static object verifyLocker = new object();

        public static MD5 md5Hash;

        static MyMD5Hash()
        {
            md5Hash = MD5.Create();
        }


        public static string GetMd5Hash(string input)
        {

            lock (hashLocker)
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }


        public static string GetMd5Hash(byte[] input)
        {

            lock (hashLocker)
            {
                byte[] data = md5Hash.ComputeHash(input);

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static bool VerifyMd5Hash(string input, string hash)
        {
            lock (verifyLocker)
            {
                string hashOfInput = GetMd5Hash(input);

                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                if (0 == comparer.Compare(hashOfInput, hash))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool VerifyMd5Hash(byte[] input, string hash)
        {
            lock (verifyLocker)
            {
                string hashOfInput = GetMd5Hash(input);

                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                if (0 == comparer.Compare(hashOfInput, hash))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
