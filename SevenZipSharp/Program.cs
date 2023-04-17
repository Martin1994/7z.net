using SevenZip;

Console.WriteLine("Started");

var path = args[0];

var stream = new FileStream(path, FileMode.Open);

var arc = new SevenZipInArchive(path, stream);

Console.WriteLine("Items: {0}", arc.Count);

Console.WriteLine("Ended");
