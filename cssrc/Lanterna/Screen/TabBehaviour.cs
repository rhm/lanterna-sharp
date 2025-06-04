using System;

namespace Lanterna.Screen
{
    /// <summary>
    /// Behaviour for how tab characters should be handled on a Screen.
    /// </summary>
    public enum TabBehaviour
    {
        Ignore,
        ConvertToOneSpace,
        ConvertToTwoSpaces,
        ConvertToThreeSpaces,
        ConvertToFourSpaces,
        ConvertToEightSpaces,
        AlignToColumn4,
        AlignToColumn8
    }

    public static class TabBehaviourExtensions
    {
        public static string ReplaceTabs(this TabBehaviour behaviour, string str, int columnIndex)
        {
            int tabPosition = str.IndexOf('\t');
            while (tabPosition != -1)
            {
                string replacement = behaviour.GetTabReplacement(columnIndex + tabPosition);
                str = str.Substring(0, tabPosition) + replacement + str.Substring(tabPosition + 1);
                tabPosition += replacement.Length;
                tabPosition = str.IndexOf('\t', tabPosition);
            }
            return str;
        }

        public static string GetTabReplacement(this TabBehaviour behaviour, int columnIndex)
        {
            int replaceCount;
            switch (behaviour)
            {
                case TabBehaviour.ConvertToOneSpace:
                    replaceCount = 1;
                    break;
                case TabBehaviour.ConvertToTwoSpaces:
                    replaceCount = 2;
                    break;
                case TabBehaviour.ConvertToThreeSpaces:
                    replaceCount = 3;
                    break;
                case TabBehaviour.ConvertToFourSpaces:
                    replaceCount = 4;
                    break;
                case TabBehaviour.ConvertToEightSpaces:
                    replaceCount = 8;
                    break;
                case TabBehaviour.AlignToColumn4:
                    replaceCount = 4 - (columnIndex % 4);
                    break;
                case TabBehaviour.AlignToColumn8:
                    replaceCount = 8 - (columnIndex % 8);
                    break;
                default:
                    return "\t";
            }
            return new string(' ', replaceCount);
        }
    }
}
