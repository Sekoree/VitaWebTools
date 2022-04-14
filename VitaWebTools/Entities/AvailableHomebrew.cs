namespace VitaWebTools.Entities
{
    public class AvailableHomebrew
    {
        public string Title { get; set; }
        public string TitleId { get; set; }

        public AvailableHomebrew(string title, string id)
        {
            Title = title;
            TitleId = id;
        }

        //JSON
        public AvailableHomebrew()
        {
            Title = "";
            TitleId = "";
        }
    }
}
