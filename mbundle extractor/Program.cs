using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mbundle_extractor
{
    
    class Program
    {
        struct mBundle
        {
        public char[] MagicNumber;
        public int numFiles;
        public long firstTable;
        public long secondTable;
            public long endSecondTable;
        }
        struct fileMbundle
        {
           public String nameFile;
            public long offsetFile;
        
        }
       
        static void Main(string[] args)
        {
            List<string> FileNames = new List<string>();
            List<long> FileOffset = new List<long>();
            mBundle Header;
            String namefile;
            if (args.Length > 0)
            {

                namefile = args[0];

                using (BigBinaryReader reader = new BigBinaryReader(File.Open(namefile, FileMode.Open)))
                {
                    long length = new System.IO.FileInfo(namefile).Length;
                    reader.BaseStream.Position = length - 16;
                    long tempFirstTable = reader.ReadInt64();
                    Header.secondTable = reader.ReadInt64();
                    reader.BaseStream.Position = tempFirstTable + 10;
                    Header.numFiles = reader.ReadInt32();
                    Header.firstTable = reader.BaseStream.Position;
                    Header.endSecondTable = length - 26;


                }

                fileMbundle[] FilesMBundle = new fileMbundle[Header.numFiles];
                using (BigBinaryReader reader = new BigBinaryReader(File.Open(namefile, FileMode.Open)))
                {



                    FileNames.Clear();
                    Header.MagicNumber = reader.ReadChars(8);

                    for (int x = 0; x < Header.numFiles; x++)
                    {
                        int next = reader.ReadByte();
                        int tempcode = next % 16;
                        if (tempcode.ToString("X") == "F")
                        {
                            next = reader.ReadByte();
                            int lengthString = reader.ReadInt32();
                            String fileName = Encoding.GetEncoding(932).GetString(reader.ReadBytes(lengthString));

                            FileNames.Add(fileName);
                        }
                        else
                        {

                            int lengthString = tempcode;
                            String fileName = Encoding.GetEncoding(932).GetString(reader.ReadBytes(lengthString));

                            FileNames.Add(fileName);


                        }
                    }




                }


                using (BigBinaryReader reader = new BigBinaryReader(File.Open(namefile, FileMode.Open)))
                {

                    //Segunda tabla de punteros.
                    reader.BaseStream.Position = Header.secondTable;
                    int islong = reader.ReadInt32();
                    bool uselong = false;
                    if (islong == 0)
                        uselong = true;
                    reader.BaseStream.Position = Header.secondTable;
                    while (reader.BaseStream.Position < Header.endSecondTable)
                    {
                        long offset;

                        if (uselong == false)
                        { offset = reader.ReadInt32(); }
                        else
                        { offset = reader.ReadInt64(); }
                        FileOffset.Add(offset);
                    }

                    int y = 0;
                    //primera tabla de punteros.
                    reader.BaseStream.Position = Header.firstTable;

                    for (int x = 0; x < (Header.numFiles * 2); x++)
                    {

                        long offset = reader.ReadInt32();


                        if (x > Header.numFiles - 1)
                        {
                            FilesMBundle[y].nameFile = FileNames[y];
                            FilesMBundle[y].offsetFile = FileOffset[(int)offset];
                            y++;
                        }
                    }


                    //writer
                    String newFolder = Path.GetFileNameWithoutExtension(namefile);
                    System.IO.Directory.CreateDirectory(newFolder);
                    for (int x = 0; x < Header.numFiles; x++)
                    {
                        Console.WriteLine( (x + 1) + " / "+ Header.numFiles);
                        Console.WriteLine("Extracting File: " + FilesMBundle[x].nameFile);
                        using (BinaryWriter writer = new BinaryWriter(File.Open(newFolder + "/" + FilesMBundle[x].nameFile, FileMode.Create)))
                        {
                            reader.BaseStream.Position = FilesMBundle[x].offsetFile;
                            //readbyte
                            int next = reader.ReadByte();
                            int tempcode = next % 16;
                            if (tempcode.ToString("X") == "F")
                            {
                                next = reader.ReadByte();
                                int lengthString = reader.ReadInt32();
                                writer.Write(reader.ReadBytes(lengthString));
                            }
                            else
                            {
                                int lengthString = tempcode;
                                String fileName = Encoding.GetEncoding(932).GetString(reader.ReadBytes(lengthString));
                                writer.Write(reader.ReadBytes(lengthString));
                            }
                            writer.Close();
                        }
                    }
                    Console.WriteLine("Extraction completed.");
                    Console.ReadLine();




                }
                


            }
            else
            { Console.WriteLine("Drag and drop here your .mbundle file.");
              Console.WriteLine("Questions?: https://github.com/darkxex");
               Console.ReadLine();
            }
        }
    }
}