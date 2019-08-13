// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

//#define ALLOCARRAY

using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;


namespace SimpleAES
{
	/// <summary>
	/// A simple AES encryption/Decryption class
	/// </summary>
	public class SimpleAES
	{
		Byte[] Key, Vector;
		private ICryptoTransform EncryptorTransform, DecryptorTransform;
		private System.Text.UTF8Encoding UTFEncoder;

		/// <summary>
		/// Initializes the simple AES class.
		/// </summary>
		/// <param name="Key">The encryption key.</param>
		/// <param name="Vector">The IV.</param>
		public SimpleAES(Byte[] i_Key, Byte[] i_Vector)
		{
			Key = i_Key;
			Vector = i_Vector;
			//This is our encryption method
			RijndaelManaged rm = new RijndaelManaged();

			//Create an encryptor and a decryptor using our encryption method, key, and vector.
			EncryptorTransform = rm.CreateEncryptor(Key, Vector);
			DecryptorTransform = rm.CreateDecryptor(Key, Vector);

			//Used to translate bytes to text and vice versa
			UTFEncoder = new System.Text.UTF8Encoding();
		}

		/// <summary>
		/// Generates an encryption key.
		/// </summary>
		/// <returns>byte[] encryption key. Store it some place safe.</returns>
		static public byte[] GenerateEncryptionKey()
		{
			//Generate a Key.
			RijndaelManaged rm = new RijndaelManaged();
			rm.GenerateKey();
			return rm.Key;
		}
		/// <summary>
		/// Generates a unique encryption vector
		/// </summary>
		/// <returns></returns>
		static public byte[] GenerateEncryptionVector()
		{
			//Generate a Vector
			RijndaelManaged rm = new RijndaelManaged();
			rm.GenerateIV();
			return rm.IV;
		}

		/// <summary>
		/// FIX - This is not a valid key storage method.  If the directory that the config files are in is
		/// compromised, the key can be illegally obtained.
		/// </summary>
		/// <returns></returns>
		static public byte[] RetrieveKey()
		{
			byte[]		abRet = null;
			string		sKey;
			int			iStrLen = 0;

			try
			{
				sKey = ConfigurationManager.AppSettings["CKey"];
				iStrLen = sKey.Length;
				if(iStrLen <= 0)
				{
					Console.WriteLine("SimpleAES.RetrieveKey ERROR:  Key was not in config file.");
				}
				else
				{
					abRet = ExtractKey(sKey);
				}
			}
			catch(Exception exc)
			{
				abRet = null;
				Console.WriteLine(exc.ToString());
			}

			return(abRet);
		}

		/// <summary>
		/// Parses the string and extracts the key.
		/// </summary>
		/// <param name="i_sKey"></param>
		/// <returns></returns>
		static public byte[] ExtractKey(string i_sKey)
		{
			byte[] abRet = null;

			try
			{
                string[] sValues = i_sKey.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                abRet = new byte[sValues.Length];

                for (int ii = 0; ii < sValues.Length; ++ii)
                {
                    abRet[ii] = (byte)int.Parse(sValues[ii]);
                }
			}
			catch (Exception exc)
			{
				abRet = null;
				Console.WriteLine(exc.ToString());
			}

			return abRet;
		}

		/// <summary>
		/// Encrypt some text and return an encrypted byte array.
		/// </summary>
		/// <param name="TextValue">The Text to encrypt</param>
		/// <returns>byte[] of the encrypted value</returns>
		public byte[] Encrypt(string TextValue)
		{
			//Translates our text value into a byte array.
			Byte[] bytes = UTFEncoder.GetBytes(TextValue);

			//Used to stream the data in and out of the CryptoStream.
			MemoryStream memoryStream = new MemoryStream();

			/*
			 * We will have to write the unencrypted bytes to the stream,
			 * then read the encrypted result back from the stream.
			 */
			#region Write the decrypted value to the encryption stream
			CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
			cs.Write(bytes,0,bytes.Length);
			cs.FlushFinalBlock();
			#endregion

#if ALLOCARRAY
			#region Read encrypted value back out of the stream
			memoryStream.Position=0;
			byte[] encrypted = new byte[memoryStream.Length];
			memoryStream.Read(encrypted,0,encrypted.Length);
			#endregion
#else
			byte[] encrypted = memoryStream.ToArray();
#endif

			//Clean up.
			cs.Close();
			memoryStream.Close();

			return encrypted;
		}

		public string Decrypt(byte[] i_abEncryptedValue)
		{
			MemoryStream	encryptedStream = null;
			CryptoStream	decryptStream = null;
			//Byte[]			decryptedBytes = null;
			//int				iRes = 0;
			string			sRet = "";

			try
			{
//Console.Error.WriteLine("### 0 ### {0}", System.Environment.StackTrace);
//Console.Error.WriteLine("SimpleAES.Decrypt: input len = {0}.", i_abEncryptedValue.Length);
				#region Write the encrypted value to the decryption stream
				encryptedStream = new MemoryStream();
				decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);

				decryptStream.Write(i_abEncryptedValue, 0, i_abEncryptedValue.Length);
				decryptStream.FlushFinalBlock();
				#endregion

#if ALLOCARRAY
				#region Read the decrypted value from the stream.
				encryptedStream.Position = 0;
//Console.Error.WriteLine("SimpleAES.Decrypt: encryptedStream len = {0}.", encryptedStream.Length);
				decryptedBytes = new Byte[encryptedStream.Length];

				iRes = encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
//Console.Error.WriteLine("SimpleAES.Decrypt: decrypted bytes read len = {0}.", iRes);
				encryptedStream.Close();
				#endregion

				sRet = UTFEncoder.GetString(decryptedBytes);
#else
				encryptedStream.Seek(0, SeekOrigin.Begin);
				StreamReader reader = new StreamReader(encryptedStream);
				sRet = reader.ReadToEnd();
#endif
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " SimpleAES.Decrypt exception: '{0}'.", exc.ToString());
			}

			return(sRet);
		}
	}
}
