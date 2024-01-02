using System.ComponentModel.DataAnnotations;

namespace CVMake;

public class Program
{
    internal static string TestPageFileName;
    private static Dictionary<string, string> settings = new Dictionary<string, string>();

    private BrowserTest browserTest;

    public static void Main(string[] args)
    {
        new Program().Run();
    }

    internal static string GetSetting(string key)
    {
        if (settings.ContainsKey(key))
        {
            return settings[key];
        }
        else
        {
            return null;
        }
    }

    private static void ReadSettings()
    {

        Console.WriteLine("Reading settings...");

        if (File.Exists("settings.ini") == false)
        {
            Console.WriteLine("settings.ini not found. Creating...");
            var template = new List<string>
            {
                "inputJob=~/Documents/Git/cv-make/CVMake/template/input.txt",
                "templatePath=~/Documents/Git/cv-make/CVMake/template/",
                "pageTemplate=page.html",
                "targetPages=2",
                "exportTo=~/Documents/Git/cv-make/CVMake/template/exports/",
                "targetSize=1055"
            };

            var writeString = "";
            foreach (string line in template)
            {
                writeString += line + "\n";
            }
            File.WriteAllText("settings.ini", writeString);
        }

        string[] lines = File.ReadAllLines("settings.ini");
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length == 2)
            {
                settings.Add(parts[0], parts[1]);
            }
            else
            {
                settings.Add(parts[0], String.Empty);
            }
        }

        if (settings.ContainsKey("inputJob") == false)
        {
            Console.WriteLine("inputJob not found in settings");
            Environment.Exit(1);
        }

        if (settings["inputJob"].StartsWith("~"))
        {
            settings["inputJob"] = settings["inputJob"].Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        if (File.Exists(settings["inputJob"]) == false)
        {
            Console.WriteLine("inputJob not found at " + settings["inputJob"]);
            Environment.Exit(1);
        }

        if (settings.ContainsKey("templatePath") == false)
        {
            Console.WriteLine("templatePath not found in settings");
            Environment.Exit(1);
        }

        if (settings["templatePath"].StartsWith("~"))
        {
            settings["templatePath"] = settings["templatePath"].Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        if (Directory.Exists(settings["templatePath"]) == false)
        {
            Console.WriteLine("templatePath not found at " + settings["templatePath"]);
            Console.WriteLine("attempting to create directory...");
            try
            {
                Directory.CreateDirectory(settings["templatePath"]);
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to create directory: " + e.Message);
                Environment.Exit(1);
            }
        }

        if (settings.ContainsKey("pageTemplate") == false)
        {
            Console.WriteLine("pageTemplate not found in settings");
            Environment.Exit(1);
        }

        if (File.Exists(settings["templatePath"] + settings["pageTemplate"]) == false)
        {
            Console.WriteLine("Page template not found at " + settings["pageTemplate"]);
            Environment.Exit(1);
        }

        if (settings.ContainsKey("exportTo") == false)
        {
            Console.WriteLine("exportTo not found in settings. Will export to current working directory.");
            settings.Add("exportTo", Directory.GetCurrentDirectory());
        }

        if (settings["exportTo"].StartsWith("~"))
        {
            settings["exportTo"] = settings["exportTo"].Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        if (Directory.Exists(settings["exportTo"]) == false)
        {
            Console.WriteLine("exportTo not found at " + settings["exportTo"]);
            Console.WriteLine("attempting to create directory...");
            try
            {
                Directory.CreateDirectory(settings["exportTo"]);
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to create directory: " + e.Message);
                Environment.Exit(1);
            }
        }

        TestPageFileName = Program.GetSetting("exportTo") + "test.html";

        if (settings.ContainsKey("targetPages") == false)
        {
            settings.Add("targetPages", "1");
        }

        if (settings["targetPages"] == String.Empty)
        {
            settings["targetPages"] = "1";
        }

        if(settings.ContainsKey("targetSize") == false)
        {
            settings.Add("targetSize", "1055");
        }
    }

    public void Run()
    {

        Console.WriteLine("CV Make");
        Program.ReadSettings();

        var inputJob = new JobInput(File.ReadAllText(Program.GetSetting("inputJob")));
        var pageTemplate = File.ReadAllLines(Program.GetSetting("templatePath") + Program.GetSetting("pageTemplate"));
        string buffer = String.Empty;
        string preContent = String.Empty;
        string postContent = String.Empty;
        var pages = new List<Page>();
        var pageElements = new List<PageElement>();
        bool mainContent = false;

        foreach (var line in pageTemplate)
        {
            if (!line.Trim().StartsWith("##")) {
                buffer += line + "\n";
            }

            if (line.Trim().Contains(".css"))
            {
                var inLineArray = line.Trim().Split('"');
                foreach (var part in inLineArray)
                {
                    if (part.Contains(".css"))
                    {
                        File.Copy(Program.GetSetting("templatePath") + part, Program.GetSetting("exportTo") + part, true);
                    }
                }
            }
            if (line.Trim().StartsWith("## CONTENT-END"))
            {
                postContent = buffer;
                buffer = "";
                mainContent = false;
                continue;
            }
            if (mainContent == true)
            {

                if (line.Trim().StartsWith("## ARRAY-"))
                {
                    var elementName = line.Replace("## ARRAY-", String.Empty).Trim().ToLower();
                    var files = Directory.GetFiles(Program.GetSetting("templatePath")).ToList<string>();
                    files = files.Order<string>().ToList<string>();
                    
                    foreach (var file in files)
                    {
                        var fileNumber = 1;

                        while(fileNumber < 20) {
                            if (file.EndsWith(elementName + fileNumber.ToString() + ".txt"))
                            {
                                var elementTemplate = File.ReadAllLines(file).ToList<string>();
                                var element = new PageElement(elementTemplate, buffer);
                                pageElements.Add(element);
                                buffer = "";
                            }
                            fileNumber++;
                        }
                    }
                    continue;
                }
                if (line.Trim().StartsWith("##"))
                {
                    var elementName = line.Replace("## ", String.Empty).Trim().ToLower();
                    var elementTemplate = File.ReadAllLines(Program.GetSetting("templatePath") + elementName + ".txt").ToList<string>();
                    var element = new PageElement(elementTemplate, buffer);
                    pageElements.Add(element);
                    buffer = "";
                    continue;
                }
            }
            if (line.Trim().StartsWith("## CONTENT-START"))
            {
                preContent = buffer;
                buffer = "";
                mainContent = true;
                continue;
            }

            
        }

        postContent = postContent + buffer;

        foreach (var element in pageElements)
        {
            element.RankListElements(inputJob);
        }

        var targetPages = Int32.Parse(settings["targetPages"]);
        for (int i = 0; i < targetPages; i++)
        {
            pages.Add(new Page(preContent, postContent));
        }

        browserTest = new BrowserTest();
        var elementsPerPage = pageElements.Count / pages.Count;
        var currentPage = 0;
        for (int i = 0; i < pageElements.Count; i++)
        {
            if (i > 0 && i % elementsPerPage == 0)
            {
                if(currentPage < (pages.Count() - 1)) {
                    currentPage++;
                }
                
            }
            pages[currentPage].AddElement(pageElements[i]);
        }

        var targetSize = Int32.Parse(Program.GetSetting("targetSize"));
        this.BalancePages(pages, targetSize);
        
        for(int i = 0; i < pages.Count(); i++) {
            pages[i].PrintToFile(Program.GetSetting("exportTo") + "page" + i.ToString() + ".html");
        }
    }

    private void BalancePages(List<Page> pages, int targetSize)
    {
        foreach(var page in pages) {
            page.MeasureSize(browserTest);
        }

        for(int currentPage = 0; currentPage < pages.Count(); currentPage++){
            var overflow = targetSize / pages[currentPage].GetNumberElements();

            if(currentPage == pages.Count() - 1) {
                // cannot overflow further
                break;
            }

            var lastSizeMeasured = pages[currentPage].GetLastSizeMeasured();
            if(lastSizeMeasured > targetSize) {
                
                while((lastSizeMeasured - targetSize) > overflow) {
                    // significant page overflow
                    // need to try and shift an element to a later page
                    if((currentPage + 1) < pages.Count) {
                        var elementToShift = pages[currentPage].PickLastElement();
                        pages[currentPage + 1].PushElement(elementToShift);
                        pages[currentPage + 1].MeasureSize(browserTest);
                        pages[currentPage].MeasureSize(browserTest);
                        lastSizeMeasured = pages[currentPage].GetLastSizeMeasured();
                    }
                }
            }
        }

        for(int i = 0; i < pages.Count(); i++){
            pages[i].MeasureSize(browserTest);
            var previousSize = pages[i].GetLastSizeMeasured();
            while(pages[i].GetLastSizeMeasured() > targetSize) {
                pages[i].ReduceElementLinesByOne();
                previousSize = pages[i].GetLastSizeMeasured();
                pages[i].MeasureSize(browserTest);

                if((i == (pages.Count() - 1)) && (pages[i].GetLastSizeMeasured() == previousSize)) {
                    // cannot overflow further
                    break;
                }
            }

            var leftOverSpace = targetSize - pages[i].GetLastSizeMeasured();

            if(leftOverSpace >= 0) {
                pages[i].AddElement(new PageElement(leftOverSpace));
            }
            
        }

    }
}