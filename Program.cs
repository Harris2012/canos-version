using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CanosVersion
{
    class Program
    {
        static void Main(string[] args)
        {

            var folder = ConfigurationManager.AppSettings["RootFolder"] ?? @"D:\TheGitlabWorkspace\savory-canos";

            var directory = new DirectoryInfo(folder);

            List<Nuspec> nuspecs = new List<Nuspec>();

            var files = directory.GetFiles("*.nuspec", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                Nuspec nuspec = new Nuspec();
                nuspec.FullName = file.FullName;

                XElement root = XElement.Load(file.FullName);

                nuspec.Id = root.Element("metadata").Element("id").Value;
                nuspec.Version = root.Element("metadata").Element("version").Value;

                var dependencies = root.Element("metadata").Element("dependencies");
                if (dependencies != null)
                {
                    foreach (var item in dependencies.Elements("dependency"))
                    {
                        var id = item.Attribute("id").Value;
                        var version = item.Attribute("version").Value;

                        nuspec.Dependencies.Add(new Nuspec { Id = id, Version = version });
                    }

                    foreach (var group in dependencies.Elements("group"))
                    {
                        foreach (var item in group.Elements("dependency"))
                        {
                            var id = item.Attribute("id").Value;
                            var version = item.Attribute("version").Value;

                            nuspec.Dependencies.Add(new Nuspec { Id = id, Version = version });
                        }
                    }
                }

                nuspecs.Add(nuspec);
            }

            foreach (var nuspec in nuspecs)
            {
                foreach (var dependency in nuspec.Dependencies)
                {
                    var items = nuspecs.Where(v => v.Id == dependency.Id).ToList();
                    if (items.Count == 0)
                    {
                        continue;
                    }
                    if (items.Count > 1)
                    {
                        Console.WriteLine("error");
                    }

                    var item = items[0];

                    if (item.Version != dependency.Version)
                    {
                        Console.WriteLine("=====================");
                        Console.WriteLine(nuspec.FullName);
                        Console.WriteLine($"{dependency.Id} version should be {item.Version} instead of {dependency.Version}");
                    }
                }

            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        class Nuspec
        {
            public string Id { get; set; }

            public string Version { get; set; }

            public string FullName { get; set; }

            public List<Nuspec> Dependencies { get; set; } = new List<Nuspec>();
        }
    }
}
