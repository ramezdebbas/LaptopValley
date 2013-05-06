using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace EcommFashion.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : EcommFashion.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Nivax Data");

            var group1 = new SampleDataGroup("Group-1",
                    "Best Laptops",
                    "Group Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "HP Envy 6 Sleekbook",
                    "",
                    "Assets/HubPage/11.png",
                    "The HP Envy Sleekbook is best known for its nearly nine hour long battery life which makes it a perfect solution for people who always find themselves on the go and are looking for something that is extremely portable. This laptop packs the AMD- Trinity Processor with an AMD Radeon HD 7500G dedicated Graphics card allowing people to run heavy duty games like COD MW 2 with a very smooth frame rate. It also does well with running other heavy duty tasks such as video and photo editing.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                   "Toshiba Satellite L855-148",
                    "",
                    "Assets/HubPage/12.png",
                    "The Toshiba Satellite L855-148 is powered by Intel’s next generation i7 processors and is a perfect alternative for people who are looking to replicate the high performance of desktop computers. This laptop is one of the most powerful machines available in the market today and the AMD Radeon HD 7670M mobile graphics card chipset acts as an icing on the cake allowing people to run high-end games.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Asus K55VD",
                    "",
                    "Assets/HubPage/13.png",
                    "Although the Asus K55VD does not come with a touchscreen nor a powerful processor, but the one thing that places it in on the wish list of every college student is its dedicated graphics card powered by the nVidia GeForce 610M chipset. And Games are not the only thing that this machine can be used for as its standard 4GB RAM can be doubled due to the availability of an extra memory slot.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Asus VivoBook S200",
                    "",
                    "Assets/HubPage/14.png",
                    "Making its way onto the number 7 spot is the Asus VivoBook 200. Be sure that you will not get a better laptop with such great specifications and a fancy touchscreen at such a low price. Windows 8 does not feel better and more responsive on any other laptop and its portable nature and slim and sleek design make it one of the most perfect laptops in the market giving you the best value for your money.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "HP Envy 14 Spectre",
                    "",
                    "Assets/HubPage/15.png",
                    "The HP Envy 14 Spectre stands out from the rest of the market particularly because of its stylish and hot design. The laptop has a solid display screen that is 14 inches wide and is capable of showing a high contrast display supporting a resolution of up to 1600 x 900 pixels. The laptop is powered with Intel’s 1.6 Ghz Core i5 processor and has a 4GB RAM making it ideal for students, businessmen and other professionals.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                    "Apple Macbook Air",
                    "",
                    "Assets/HubPage/16.png",
                    "Occupying the number five spot in our Top Ten Best Laptops for 2013 is the Apple Macbook Air. Although the Macbook Air was released way back in 2008, it has continued to stay as a fan favorite probably because of its Apple tag. The laptop is powered by none other than Intel’s core i5 processor with Turbo Boost Technology and a 4GB RAM. The Macbook Air is available in two different models which include the 13.3 inches screen and the 11.6 inches screen. The 11.6 inches model roughly weighs around 2.4 pounds while the 13.3 inch model weighs in at 2.9 pounds making the machines extremely portable and easy to move and carry around.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);

             var group2 = new SampleDataGroup("Group-2",
                    "Smart Laptops",
                    "Group Subtitle: 2",
                    "Assets/LightGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
					"Apple Macbook Pro",
					"",
                    "Assets/HubPage/21.png",
                    "The Apple Macbook Pro occupies our number four spot having repeatedly featured in our top ten lists for a number of years. This is one of the most fantastic products from Apple being extremely durable and having all the latest features. It is an extra light and extra thin laptop which comes with a Retina Display and can give a battery timing of up to 7 hours with full charge. The laptop comes with an Intel Core i7 Processor and dedicated graphics card chipset from nVidia.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Asus ZenBook Prime",
                    "",
                    "Assets/HubPage/22.png",
                    "The Asus ZenBook Prime is a long lasting, light, thin and powerful ultra-book machine that very few models in the market can compete with. It features a full HD 1080p display support and was the first laptop of its kind to support such a high resolution on a 13 inch screen. Superb color reproduction and perfect viewing angles powered by an Intel i7 processor and 7 hours of battery make this the most desirable laptop in the market today.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                     "Dell XPS 12",
                    "",
                    "Assets/HubPage/23.png",
                    "If you are looking for an ultimate laptop-table hybrid, look no further than the Dell XPS 12. The machine is capable of supporting 1080p resolution and by pushing the screen from behind, the 12.5 inches display screen will spin around to form a tablet. The core i7 processor with 8GB RAM is more than enough to run any task with high speed and performance.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                    "Lenovo ThinkPad X1 Carbon",
                   "",
                   "Assets/HubPage/24.png",
                   "The second business Ultrabook from Lenovo has proved to be an absolute sensation and integrates the good looks, performance, high battery life and strong performance into one complete and highly desirable package. The design is professional, sleek and slim and the machine has been designed to perform some serious work.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-5",
                    "Origin EON17-SLX",
                   "",
                   "Assets/HubPage/25.png",
                   "When financial limits are tossed to the wind, you'll find laptops that offer higher-than-high-end components, opting for absurdly powerful processors instead of the slimmer, battery-friendly CPUs used elsewhere. You'll get not one, but two discrete graphics cards, and you'll see bells and whistles like nobody's business. You'll also pay through the nose. Case in point: the Origin EON17-SLX, which costs more than the first two cars I drove (combined). Loaded up with an overclocked Intel Core i7-3940XM quad-core and dual Nvidia GeForce GTX 680M graphics processors, it has some serious gaming chops—and snags our Editors' Choice award for high-end gaming laptops. Oh, and did we mention it comes with Windows 8?",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-6",
                    "Microsoft Surface Windows 8 Pro",
                   "",
                   "Assets/HubPage/26.png",
                   "Tablets with mobile operating systems like Android, iOS, or Windows RT are perfectly adequate if your tasks center on the Internet. But if you need compatibility with older x86 programs in a slate tablet form factor, then you need one running Windows 8. Of the handful introduced lately, the Microsoft Surface Windows 8 Pro is the one to beat.",
                   ITEM_CONTENT,
                   group2));
            this.AllGroups.Add(group2); 


        }
    }
}
