namespace point_cloud_analyzer_web.Models
{
    public class CustomModule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool NeedUploadFile { get; set; }
        public bool NeedCommand { get; set; }

        public CustomModule()
        {

        }
    }
}
