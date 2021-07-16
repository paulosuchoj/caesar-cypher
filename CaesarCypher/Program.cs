using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace CaesarCypher
{
  public class RequestContent
  {
    public int numero_casas { get; set; }
    public string token { get; set; }
    public string cifrado { get; set; }
    public string decifrado { get; set; }
    public string resumo_criptografico { get; set; }
  }

  class Program
  {
    static async System.Threading.Tasks.Task Main(string[] args)
    {
      int shift;
      string decipheredMessage = "", json, hash, jsonOutput;
      char[] cipherCharArray;
      byte[] sourceBytes, hashBytes;

      WebRequest getRequest = WebRequest.CreateHttp("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=c653ceb64b472ba118600b49511332d6f6ed0c3e");
      getRequest.Method = "GET";

      using (var response = getRequest.GetResponse())
      {
        var dataStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(dataStream);
        object objResponse = reader.ReadToEnd();

        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "answer.json"), objResponse.ToString(), Encoding.UTF8);

        json = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "answer.json"));

        RequestContent requestContent = JsonConvert.DeserializeObject<RequestContent>(json);

        shift = requestContent.numero_casas;
        cipherCharArray = requestContent.cifrado.ToLower().ToCharArray();

        //Separar em outro método
        for (var i = 0; i < cipherCharArray.Length; i++)
        {
          var character = (int)cipherCharArray[i];

          if((character >= 97) && (character <= 122))
          {
            decipheredMessage += (char)((character - 97 - shift + 26) % 26 + 97);
          } else
          {
            decipheredMessage += cipherCharArray[i];
          }
        }

        //Separar em outro método
        SHA1 sha1Hash = SHA1.Create();
        sourceBytes = Encoding.UTF8.GetBytes(decipheredMessage.ToString());
        hashBytes = sha1Hash.ComputeHash(sourceBytes);
        hash = BitConverter.ToString(hashBytes).Replace("-", "");

        requestContent.resumo_criptografico = hash.ToLower();

        requestContent.decifrado = decipheredMessage.ToString();

        jsonOutput = JsonConvert.SerializeObject(requestContent);

        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "answer.json"), jsonOutput, Encoding.UTF8);

        dataStream.Close();
        response.Close();
      }
      var answerFile = File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "answer.json"));

      HttpClient httpClient = new HttpClient();
      MultipartFormDataContent postContent = new MultipartFormDataContent();

      ByteArrayContent byteContent = new ByteArrayContent(answerFile);
      postContent.Add(byteContent, "answer", "filename.ext");

      var response = await httpClient.PostAsync("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=c653ceb64b472ba118600b49511332d6f6ed0c3e", postContent);
      var responsestr = response.Content.ReadAsStringAsync().Result;

      Console.WriteLine(responsestr);
    }
  }
}
