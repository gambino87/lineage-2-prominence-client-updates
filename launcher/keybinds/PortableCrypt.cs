/*
 * Copyright (c) 2021 acmi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Globalization;
using System.Text;
namespace LocalL2Keys {
// Port of acmi/L2crypt's MIT-licensed 413 codec; see LICENSE-L2crypt.txt.
public static class PortableCrypt {
 static readonly BigInteger Modulus=BigInteger.Parse("075b4d6de5c016544068a1acf125869f43d2e09fc55b8b1e289556daf9b8757635593446288b3653da1ce91c87bb1a5c18f16323495c55d7d72c0890a83f69bfd1fd9434eb1c02f3e4679edfa43309319070129c267c85604d87bb65bae205de3707af1d2108881abb567c3b3d069ae67c3a4c6a3aa93d26413d4c66094ae2039",NumberStyles.HexNumber);
 static readonly BigInteger Public=BigInteger.Parse("030b4c2d798d47086145c75063c8e841e719776e400291d7838d3e6c4405b504c6a07f8fca27f32b86643d2649d1d5f124cdd0bf272f0909dd7352fe10a77b34d831043d9ae541f8263c6fe3d1c14c2f04e43a7253a6dda9a8c1562cbd493c1b631a1957618ad5dfe5ca28553f746e2fc6f2db816c7db223ec91e955081c1de65",NumberStyles.HexNumber);
 static readonly byte[] Header=Encoding.Unicode.GetBytes("Lineage2Ver413");
 static byte[] Transform(byte[] block,BigInteger exponent){
  byte[] little=new byte[129];for(int i=0;i<128;i++)little[i]=block[127-i];
  byte[] result=BigInteger.ModPow(new BigInteger(little),exponent,Modulus).ToByteArray(), output=new byte[128];
  for(int i=0;i<Math.Min(128,result.Length);i++)output[127-i]=result[i];return output;
 }
 static uint Adler(byte[] bytes){uint a=1,b=0;foreach(byte x in bytes){a=(a+x)%65521;b=(b+a)%65521;}return (b<<16)|a;}
 static uint Crc(byte[] bytes){uint c=0xffffffff;foreach(byte x in bytes){c^=x;for(int i=0;i<8;i++)c=(c&1)!=0?(c>>1)^0xedb88320:c>>1;}return ~c;}
 public static byte[] Encode(byte[] plain){
  byte[] compressed;using(var z=new MemoryStream()){
   z.Write(BitConverter.GetBytes(plain.Length),0,4);z.WriteByte(0x78);z.WriteByte(0x9c);
   using(var deflate=new DeflateStream(z,CompressionMode.Compress,true))deflate.Write(plain,0,plain.Length);
   uint adler=Adler(plain);for(int i=3;i>=0;i--)z.WriteByte((byte)(adler>>(8*i)));compressed=z.ToArray();
  }
  using(var output=new MemoryStream()){
   output.Write(Header,0,Header.Length);
   for(int offset=0;offset<compressed.Length;offset+=124){int count=Math.Min(124,compressed.Length-offset);byte[] block=new byte[128];block[3]=(byte)count;Array.Copy(compressed,offset,block,128-count-((124-count)%4),count);block=Transform(block,Public);output.Write(block,0,128);}
   uint crc=Crc(output.ToArray());byte[] footer=new byte[20];Array.Copy(BitConverter.GetBytes(crc),0,footer,12,4);output.Write(footer,0,footer.Length);return output.ToArray();
  }
 }
 public static byte[] Decode(byte[] encoded){
  if(encoded.Length<Header.Length+148 || !encoded.Take(Header.Length).SequenceEqual(Header) || (encoded.Length-Header.Length-20)%128!=0)throw new InvalidDataException("Unsupported client encryption format.");
  byte[] data;using(var payload=new MemoryStream()){
   for(int offset=Header.Length;offset<encoded.Length-20;offset+=128){byte[] block=new byte[128];Array.Copy(encoded,offset,block,0,128);block=Transform(block,new BigInteger(29));int count=block[3];if(count>124)throw new InvalidDataException("Invalid encrypted block.");payload.Write(block,128-count-((124-count)%4),count);}
   data=payload.ToArray();
  }
  if(data.Length<10)throw new InvalidDataException("Invalid compressed client data.");
  int size=BitConverter.ToInt32(data,0);if(size<0 || size>64*1024*1024)throw new InvalidDataException("Client data is too large.");
  using(var input=new MemoryStream(data,6,data.Length-10))using(var deflate=new DeflateStream(input,CompressionMode.Decompress))using(var output=new MemoryStream()){
   byte[] buffer=new byte[8192];int count;while((count=deflate.Read(buffer,0,buffer.Length))>0){if(output.Length+count>size)throw new InvalidDataException("Client data size mismatch.");output.Write(buffer,0,count);}
   byte[] result=output.ToArray();uint expected=0;for(int i=data.Length-4;i<data.Length;i++)expected=(expected<<8)|data[i];if(result.Length!=size || Adler(result)!=expected)throw new InvalidDataException("Client data checksum failed.");return result;
  }
 }
}
}
