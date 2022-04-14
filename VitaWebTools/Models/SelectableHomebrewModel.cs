using VitaWebTools.Entities;

namespace VitaWebTools.Models
{
    public class SelectableHomebrewModel
    {
        public bool Selected { get; set; }
        public string Title { get; set; }
        public string TitleId { get; set; }

        public SelectableHomebrewModel(AvailableHomebrew hb)
        {
            Title = hb.Title;
            TitleId = hb.TitleId;
            Selected = false;
        }
    }
}
