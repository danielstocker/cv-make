namespace CVMake;
public class Page
{

    private string preContent = "";
    private string postContent = "";
    private int lastLength = 0;
    private int lastLowestElement = -1;
    private List<PageElement> pageElements = new List<PageElement>();

    public Page(String pre, String post)
    {
        if (pre == null)
        {
            pre = String.Empty;
        }

        if (post == null)
        {
            post = String.Empty;
        }

        preContent = pre;
        postContent = post;

    }

    public Page()
    {
        preContent = String.Empty;
        postContent = String.Empty;
    }

    internal void AddElement(PageElement element)
    {
        pageElements.Add(element);
    }

    internal void ClearPage()
    {
        pageElements.Clear();
    }

    public string GetContent()
    {
        string content = preContent;
        foreach (var element in pageElements)
        {
            content += element.GetContent();
        }
        content += postContent;
        return content;
    }

    internal void MeasureSize(BrowserTest browser)
    {
        File.WriteAllText(Program.TestPageFileName, this.GetContent());
        lastLength = browser.GetPageLength(Program.TestPageFileName);
    }

    internal void PrintToFile(string fileName)
    {
        File.WriteAllText(fileName, this.GetContent());
    }

    internal int GetLastSizeMeasured()
    {
        return lastLength;
    }

    internal PageElement PickLastElement()
    {
        var pickedElement = pageElements[this.pageElements.Count - 1];
        pageElements.RemoveAt(this.pageElements.Count - 1);
        return pickedElement;
    }

    internal void PushElement(PageElement elementToShift)
    {
        var newPageElements = new List<PageElement>();
        newPageElements.Add(elementToShift);
        foreach (var element in pageElements)
        {
            newPageElements.Add(element);
        }
        pageElements = newPageElements;
    }

    internal void ReduceElementLinesByOne() {
        PageElement targetElement = this.GetPageElementWithTheLowestRank();
        var target = targetElement.GetNumElementTarget();
        var cols = targetElement.GetNumColumns();
            
        targetElement.SetNumElementTarget(target-cols);
        
    }

    internal int GetNumberElements()
    {
        return pageElements.Count;
    }

    private PageElement GetPageElementWithTheLowestRank() {

        PageElement lowestElement = null;
        int i = 1;
        for(i = 1; i < pageElements.Count; i++) {
            lowestElement = pageElements[pageElements.Count - i];
            if(!lowestElement.GetDropped()) {
                break;
            }
        }
        
        for(i = 0; i < pageElements.Count; i++){
            var element = pageElements[i];

            if(element.GetDropped()) {
                continue;
            }
            
            if(lowestElement == null) {
                lowestElement = element;
                lastLowestElement = i;
            } 
            if(lowestElement.GetHarmonizedRank() > element.GetHarmonizedRank()) {
                if(i == lastLowestElement) {
                    continue;
                }
                lastLowestElement = i;
                lowestElement = element;
            }
        }

        if(lastLowestElement == -1) {
            lastLowestElement = pageElements.Count - 1;
        }

        return lowestElement;
    }

    
}

