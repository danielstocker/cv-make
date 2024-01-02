
using System.Data;
using System.Reflection.Metadata;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace CVMake;
public class PageElement
{
    private string preContent = "";

    private string staticContent = "";
    private int numListElementTarget = 0;
    private bool strictListElementTarget = false;
    private bool doNotUseBullets = false;
    private bool isDroppable = false;
    private bool dropped = false;
    private int numColumns = 1;
    private int minListElements = -1;
    private List<ListElement> listElements = new List<ListElement>();

    public PageElement(int space) {
        preContent = "<div style=\"width: 100%; display: block; height: " + space + "px;\">&nbsp;</div>";
        doNotUseBullets = true;
        strictListElementTarget = true;
        numColumns = 1;
        numListElementTarget = 0;
    }

    public PageElement(List<string> options, string pre)
    {
        if (pre == null)
        {
            pre = String.Empty;
        }
        preContent = pre;

        if (options == null || options.Count == 0)
        {
            throw new InvalidOperationException("Options cannot be null or empty");
        }
        
        var currentOption = String.Empty;
        var readItem = false;
        var itemContent = String.Empty;
        foreach(var line in options) {
            if(line.Trim().StartsWith("##")) {
                if(readItem) {
                    listElements.Add(new ListElement(itemContent, 0));
                    readItem = false;
                    itemContent = String.Empty;
                }

                currentOption = line.Replace("##", String.Empty).Trim().ToLower();
                continue;
            }
            if(currentOption.StartsWith("columns")) {
                numColumns = Int32.Parse(line);
                currentOption = String.Empty;
            }

            if(currentOption.StartsWith("intro")) {
                staticContent = staticContent + line;
            }

            if(currentOption.StartsWith("item")) {   
                readItem = true;
                itemContent = itemContent + line;
            }

            if(currentOption.StartsWith("listitems")) {
                if(line.StartsWith("strict")) {
                    strictListElementTarget = true;
                    continue;
                }

                if(line.StartsWith("no-bullet")) {
                    doNotUseBullets = true;
                    continue;
                }

                if(line.StartsWith("droppable")) {
                    isDroppable = true;
                    continue;
                }

                if(line.StartsWith("min-")) {
                    minListElements = Int32.Parse(line.Replace("min-", String.Empty));
                    continue;
                }

                int target = -1;

                if(!Int32.TryParse(line, out target)) {
                } else {
                    numListElementTarget = target;
                }
            }
        }

        if(readItem) {
            listElements.Add(new ListElement(itemContent, 0));
            readItem = false;
            itemContent = String.Empty;
        }

        for(int i = 0; i < listElements.Count; i++) {
            listElements[i].SetRank(listElements.Count - i);
        }
    }

    internal string GetContent() {
        return GetContent(numListElementTarget);
    }

    internal string GetPreContent() {
        return preContent;
    }

    internal string GetContent(int maxBullets)
    {
        if(dropped) {
            return String.Empty;
        }

        String content = "";

        // order list elements by their rank
        listElements = listElements.OrderByDescending(o => o.GetRank()).ToList();
        
        content = content + preContent;
        content = content + staticContent;

        if(listElements.Count < 1) {
            return content;
        }

        var columnWidth = 100 / numColumns;
        content = content + "<table class='table-element'><tr><td class='listitem' width='" + columnWidth + "%'>";

        if(!this.doNotUseBullets && listElements.Count > 0) {
            content = content + "<ul>";

        }

        var bulletsAdded = 0;
        foreach(var element in listElements) {
            if(bulletsAdded >= maxBullets) {
                break;
            }

            if(numColumns > 1 && bulletsAdded > 0 && bulletsAdded % ((maxBullets + 1) / numColumns) == 0) {
                content = content + "</ul></td><td class='listitem' width='" + columnWidth + "%'><ul>";
            }

            if(this.doNotUseBullets) {
                content = content + element.GetContent();
            } else {
                content = content + "<li>" + element.GetContent() + "</li>";
            }
            

            bulletsAdded++;
        }

        if(!this.doNotUseBullets && bulletsAdded > 0) {
            content = content + "</ul>";

        }

        content = content + "</td></tr></table>";

        return content;
    }

    internal void RankListElements(JobInput input) {
        foreach(var element in listElements) {
            element.SetRank(element.GetRank() + input.RateInput(element.GetContent()));
        }
    }

    internal int GetNumListElements() {
        return listElements.Count;
    }

    internal int GetNumElementTarget() {
        return numListElementTarget;
    }

    internal int GetNumColumns() {
        return numColumns;
    }

    internal void SetNumElementTarget(int newTarget) {
        if(newTarget >= numColumns) {
            numListElementTarget = newTarget;

            if(minListElements >= numListElementTarget) {
                strictListElementTarget = true;
            }

        } else {
            if(isDroppable) {
                dropped = true;
            } else {
                strictListElementTarget = true;
            }
        }
    }

    internal int GetHarmonizedRank()
    {
        var bulletsAdded = 0;
        var overallRank = 0;
        foreach(var element in listElements) {
            if(bulletsAdded >= this.numListElementTarget) {
                break;
            }

            overallRank += element.GetRank();
            overallRank -= (bulletsAdded - 1);
            bulletsAdded++;
        }

        if(strictListElementTarget) {
            overallRank = overallRank + 75;
        }

        return overallRank / bulletsAdded;
    }

    internal bool GetDropped()
    {
        return dropped;
    }
}

