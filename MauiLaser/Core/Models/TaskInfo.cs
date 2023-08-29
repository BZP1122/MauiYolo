using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honor_Maui.Core.Models
{
    public partial class TaskInfo: BaseModel
    {
        [ObservableProperty]
        string taskName;
        [ObservableProperty]
        int excutedNums;
    }
}
