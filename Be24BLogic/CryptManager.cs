using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace Be24BLogic
{



    
    public   class Cryption
    {



        public static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
                return Encoding.ASCII.GetString(result);
            }
        }


        public static string GetMd5Hash(  string input)
        {
            var md5 = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }





        public static bool VerifyMd5Hash(  string input, string hash)
        {
            
            string hashOfInput = GetMd5Hash(  input);

            
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




        public static string Encrypt(string valueToEncrypt, string symmetricKey, string initializationVector)
        {
            string returnValue = "";

            var text = valueToEncrypt;
            var buffer = Encoding.UTF8.GetBytes(text);

            var iv = GetRandomData(128);//Encoding.UTF8.GetBytes("иволга");//GetRandomData(128);
            var keyAes = GetRandomData(256); //Encoding.UTF8.GetBytes("подснежник"); //GetRandomData(256);


            byte[] result;
            using (var aes = Aes.Create())
            {
                aes.Key = keyAes;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }
            returnValue= Encoding.UTF8.GetString(result);
            return returnValue;
        }

        public static string Decrypt(string valueToEncrypt, string symmetricKey, string initializationVector)
        {
            string returnValue = "";

            var text = valueToEncrypt;
            var buffer = Encoding.UTF8.GetBytes(text);

            var iv = GetRandomData(128); //Encoding.UTF8.GetBytes("иволга");//GetRandomData(128);
            var keyAes = GetRandomData(256); //Encoding.UTF8.GetBytes("подснежник"); //GetRandomData(256);


            byte[] result;
            using (var aes = Aes.Create())
            {
                aes.Key = keyAes;
                ICryptoTransform decryptor = aes.CreateDecryptor(keyAes, iv);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(buffer))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            returnValue = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
             //   returnValue = Encoding.UTF8.GetString(result);
            return returnValue;
        }


        private static byte[] GetRandomData(int bits)
        {
            var result = new byte[bits / 8];
            // RandomNumberGenerator.Create().GetBytes(result);
            for (int i= 0; i< result.Length ; i++)
            {
                result[i] = 1;
            }
            return result;
        }



        ////members of the Cryption 
        ////algorithm type in my case it’s RijndaelManaged
        // ICryptoTransform 
        //CryptoStream  cs = new CryptoStream() 
        //private RijndaelManaged Algorithm;
        ////memory stream
        //private MemoryStream memStream;
        ////ICryptoTransform interface
        //private ICryptoTransform EncryptorDecryptor;
        ////CryptoStream
        //private CryptoStream crStream;
        ////Stream writer and Stream reader
        //private StreamWriter strWriter;
        //private StreamReader strReader;
        ////internal members
        //private string m_key;
        //private string m_iv;
        ////the Key and the Inicialization Vector
        //private byte[] key;
        //private byte[] iv;
        ////password view
        //private string pwd_str;
        //private byte[] pwd_byte;
        ////Constructor
        //public Cryption(string key_val, string iv_val)
        //{
        //    key = new byte[32];
        //    iv = new byte[32];

        //    int i;
        //    m_key = key_val;
        //    m_iv = iv_val;
        //    //key calculation, depends on first constructor parameter
        //    for (i = 0; i < m_key.Length; i++)
        //    {
        //        key[i] = Convert.ToByte(m_key[i]);
        //    }
        //    //IV calculation, depends on second constructor parameter
        //    for (i = 0; i < m_iv.Length; i++)
        //    {
        //        iv[i] = Convert.ToByte(m_iv[i]);
        //    }

        //}
        ////Encrypt method implementation
        //public string Encrypt(string s)
        //{
        //    //new instance of algorithm creation
        //    Algorithm = new RijndaelManaged();

        //    //setting algorithm bit size
        //    Algorithm.BlockSize = 256;
        //    Algorithm.KeySize = 256;

        //    //creating new instance of Memory stream
        //    memStream = new MemoryStream();

        //    //creating new instance of the Encryptor
        //    EncryptorDecryptor = Algorithm.CreateEncryptor(key, iv);

        //    //creating new instance of CryptoStream
        //    crStream = new CryptoStream(memStream, EncryptorDecryptor,
        //    CryptoStreamMode.Write);

        //    //creating new instance of Stream Writer
        //    strWriter = new StreamWriter(crStream);

        //    //cipher input string
        //    strWriter.Write(s);

        //    //clearing buffer for currnet writer and writing any 
        //    //buffered data to //the underlying device
        //    strWriter.Flush();
        //    crStream.FlushFinalBlock();

        //    //storing cipher string as byte array 
        //    pwd_byte = new byte[memStream.Length];
        //    memStream.Position = 0;
        //    memStream.Read(pwd_byte, 0, (int)pwd_byte.Length);

        //    //storing cipher string as Unicode string
        //    pwd_str = new UnicodeEncoding().GetString(pwd_byte);

        //    return pwd_str;
        //}

        ////Decrypt method implementation 
        //public string Decrypt(string s)
        //{
        //    //new instance of algorithm creation
        //    Algorithm = new RijndaelManaged();

        //    //setting algorithm bit size
        //    Algorithm.BlockSize = 256;
        //    Algorithm.KeySize = 256;

        //    //creating new Memory stream as stream for input string      
        //    MemoryStream memStream = new MemoryStream(
        //       new UnicodeEncoding().GetBytes(s));

        //    //Decryptor creating 
        //    ICryptoTransform EncryptorDecryptor =
        //        Algorithm.CreateDecryptor(key, iv);

        //    //setting memory stream position
        //    memStream.Position = 0;

        //    //creating new instance of Crupto stream
        //    CryptoStream crStream = new CryptoStream(
        //        memStream, EncryptorDecryptor, CryptoStreamMode.Read);

        //    //reading stream
        //    strReader = new StreamReader(crStream);

        //    //returnig decrypted string
        //    return strReader.ReadToEnd();
        //}
    }
}
