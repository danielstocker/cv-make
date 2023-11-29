namespace CVMake;

public class JobInput
{
    private Dictionary<string, int> wordCloud = new Dictionary<string, int>();

    public JobInput(string jobDescription) {
        wordCloud = MakeWordCloud(jobDescription);
    }

    public int RateInput(string input) {
        var output = 0;
        var inputCloud = MakeWordCloud(input);

        foreach(var key in inputCloud.Keys) {
            if(wordCloud.ContainsKey(key)) {
                if(wordCloud[key] == -1) {
                    continue;
                }
                output = output + wordCloud[key];
            }
        }

        return output;
    }

    private Dictionary<string, int> MakeWordCloud(string input) {
        var output = new Dictionary<string, int>();

        var words = input.Split(' ');

        foreach(var word in words) {
            var target = word.ToLower().Trim();
            if(output.ContainsKey(target)) {
                if(output[target] == -1) {
                    continue;
                }

                output[target] += 1;

                if(output[target] > 10) {
                    output[target] = -1;
                }
            } else {
                output.Add(target, 1);
            }
        }

        return output;
    }
}
