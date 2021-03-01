using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace ECCLibrary
{
    [Menu("Eel's Creature Creator Settings")]
    public class ECCConfig : ConfigFile
    {
        [Slider("ECC Master volume", 0f, 100f, Step = 1f, DefaultValue = 100f, Tooltip = "Not influenced by the in-game sound setting.")]
        public float VolumeNew;
        [Toggle("ECC Verbose Messages", Tooltip = "Whether to display ECC log messages & errors on the screen.")]
        public bool ECCLogMessages;
    }
}
