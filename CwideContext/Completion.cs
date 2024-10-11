using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PluginCore.Managers;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore.Localization;
using PluginCore.Controls;
using PluginCore.Helpers;
using PluginCore;
using ASCompletion.Completion;
using System.Windows.Forms;
using ASCompletion;

using ScintillaNet;
using System.Drawing;

namespace CwideContext
{

    class Completion
    {

        
//        static public string word = "";
        static public bool faded = false;


         static public void ShowNx(List<ICompletionListItem> itemList, Boolean autoHide, String select) {
         //  CompletionList.Show(itemList,autoHide,select);
         TraceManager.Add("Select!! " + select);
             ShowCL(itemList,autoHide,select);
        }








     static public event InsertedTextHandler OnInsert;
        static public event InsertedTextHandler OnCancel;

        /// <summary>
        /// Properties of the class 
        /// </summary> 
        private static System.Timers.Timer tempo;
        private static System.Timers.Timer tempoTip;
        private static System.Windows.Forms.ListBox completionList;

        private static bool disableSmartMatch;
        private static ICompletionListItem currentItem;
        private static List<ICompletionListItem> allItems;
        private static Boolean exactMatchInList;
        private static Boolean smartMatchInList;
        private static Boolean autoHideList;
        private static Boolean noAutoInsert;
        private static Boolean isActive;
        internal static Boolean listUp;
        private static Boolean fullList;
        private static Int32 startPos;
        private static Int32 currentPos;
        private static Int32 lastIndex;
        private static String currentWord;
        private static String word;
        private static Boolean needResize;
        private static String widestLabel;
        private static long showTime;
        private static ICompletionListItem defaultItem;
      
        /// <summary>
        /// Set to 0 after calling .Show to keep the completion list active 
        /// when the text was erased completely (using backspace)
        /// </summary>
        public static Int32 MinWordLength;
          

       struct ItemMatch
    {
        public int Score;
        public ICompletionListItem Item;

        public ItemMatch(int score, ICompletionListItem item)
        {
            Score = score;
            Item = item;
        }
    }
        
       // private static System.Windows.Forms.ListBox completionList;
       //   private static System.Windows.Forms.ListBox completionList;

      public static void CreateControl(IMainForm mainForm) {
   
            tempo = new System.Timers.Timer();
            tempo.SynchronizingObject = (Form)mainForm;
            tempo.Elapsed += new System.Timers.ElapsedEventHandler(DisplayList);
            tempo.AutoReset = false;
            tempoTip = new System.Timers.Timer();
            tempoTip.SynchronizingObject = (Form)mainForm;
            tempoTip.Elapsed += new System.Timers.ElapsedEventHandler(UpdateTip);
            tempoTip.AutoReset = false;
            tempoTip.Interval = 800;
            
            completionList = new ListBox();
            completionList.Font = new System.Drawing.Font(PluginBase.Settings.DefaultFont, FontStyle.Regular);
            completionList.Visible = false;
            completionList.Location = new Point(400,200);
            completionList.ItemHeight = completionList.Font.Height + 2;
            completionList.Size = new Size(180, 100);
            completionList.DrawMode = DrawMode.OwnerDrawFixed;
            completionList.DrawItem += new DrawItemEventHandler(CLDrawListItem);
            completionList.Click += new EventHandler(CLClick);
            completionList.DoubleClick += new EventHandler(CLDoubleClick);
            mainForm.Controls.Add(completionList);
        }

          static public void UpdateTip(Object sender, System.Timers.ElapsedEventArgs e)
        {
            tempoTip.Stop();
            if (currentItem == null || faded)
                return;

            UITools.Tip.SetText(currentItem.Description ?? "", false);
            UITools.Tip.Redraw(false);

            int rightWidth = ((Form)PluginBase.MainForm).ClientRectangle.Right - completionList.Right - 10;
            int leftWidth = completionList.Left;

            Point posTarget = new Point(completionList.Right, completionList.Top);
            int widthTarget = rightWidth;
            if (rightWidth < 220 && leftWidth > 220)
            {
                widthTarget = leftWidth;
                posTarget = new Point(0, completionList.Top);
            }

            UITools.Tip.Location = posTarget;
            UITools.Tip.AutoSize(widthTarget, 500);

            if (widthTarget == leftWidth)
                UITools.Tip.Location = new Point(completionList.Left - UITools.Tip.Size.Width, posTarget.Y);

            UITools.Tip.Show();
        }

          static private void CLDrawListItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            ICompletionListItem item = completionList.Items[e.Index] as ICompletionListItem;
            e.DrawBackground();
            Color fore = PluginBase.MainForm.GetThemeColor("CompletionList.ForeColor", SystemColors.WindowText);
            Color sel = PluginBase.MainForm.GetThemeColor("CompletionList.SelectedTextColor", SystemColors.HighlightText);
            bool selected = (e.State & DrawItemState.Selected) > 0;
            Brush textBrush = (selected) ? new SolidBrush(sel) : new SolidBrush(fore);
            Brush packageBrush = new SolidBrush(PluginBase.MainForm.GetThemeColor("CompletionList.PackageColor", Color.Gray));
            Rectangle tbounds = new Rectangle(ScaleHelper.Scale(18), e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
            if (item != null)
            {
                Graphics g = e.Graphics;
                float newHeight = e.Bounds.Height - 2;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(item.Icon, 1, e.Bounds.Top + ((e.Bounds.Height - newHeight) / 2), newHeight, newHeight);
                int p = item.Label.LastIndexOf('.');
                if(selected) {
                     packageBrush = new SolidBrush(PluginBase.MainForm.GetThemeColor("CompletionList.PackageColor", Color.DarkSlateGray));
                }

               // if (p > 0 && !selected)
                if (p > 0)
                {
                    string package = item.Label.Substring(0, p + 1);
                    g.DrawString(package, e.Font, packageBrush, tbounds, StringFormat.GenericDefault);
                    int left = tbounds.Left + DrawHelper.MeasureDisplayStringWidth(e.Graphics, package, e.Font) - 2;
                    if (left < tbounds.Right) g.DrawString(item.Label.Substring(p + 1), e.Font, textBrush, left, tbounds.Top, StringFormat.GenericDefault);
                }
                else g.DrawString(item.Label, e.Font, textBrush, tbounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
            if ((item != null) && ((e.State & DrawItemState.Selected) > 0))
            {
                UITools.Tip.Hide();
                /*
                currentItem = item;
                tempoTip.Stop();
                tempoTip.Start();*/
            }
        }


         static private void CLClick(Object sender, System.EventArgs e)
        {
            ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable)
            {
                Hide();
                return;
            }
            doc.SciControl.Focus();
        }


         static private void CLDoubleClick(Object sender, System.EventArgs e)
        {
        

                /*
            ScintillaControl sci = doc.SciControl;
            sci.Focus();
            ReplaceText(sci, '\0');
      */
            ValidateSelection();

        }


          static public void ValidateSelection()
        {
            ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable)
            {
                Hide();
                return;
            }
            ScintillaControl sci = doc.SciControl;
            sci.Focus();
            ReplaceText(sci, '\0');
        }


          static public bool ReplaceText(ScintillaControl sci, char trigger)
        {
            return ReplaceText(sci, "", trigger);
        }

         static public bool ReplaceText(ScintillaControl sci, String tail, char trigger)
        {
            sci.BeginUndoAction();
            try
            {
                String triggers = PluginBase.Settings.InsertionTriggers ?? "";
                if (triggers.Length > 0 && Regex.Unescape(triggers).IndexOf(trigger) < 0) return false;

                ICompletionListItem item = null;
                if (completionList.SelectedIndex >= 0)
                {
                    item = completionList.Items[completionList.SelectedIndex] as ICompletionListItem;
                }
                Hide();
                if (item != null)
                {
                    String replace = fItemName( item);
                    //String replace = item.Value;
                    if (replace != null)
                    {
                        TraceManager.Add("startPos " + startPos + " sci.CurrentPos  " + sci.CurrentPos + " currentWord.Length " + currentWord);
                      //  sci.SetSel(startPos, sci.CurrentPos);
                        sci.SetSel( sci.CurrentPos - currentWord.Length, sci.CurrentPos);
                        if (word != null && tail.Length > 0)
                        {
                            if (replace.StartsWith(word, StringComparison.OrdinalIgnoreCase) && replace.IndexOfOrdinal(tail) >= word.Length)
                            {
                                replace = replace.Substring(0, replace.IndexOfOrdinal(tail));
                            }
                        }
                        sci.ReplaceSel(replace);
                        if (OnInsert != null) OnInsert(sci, startPos, replace, trigger, item);
                        if (tail.Length > 0) sci.ReplaceSel(tail);
                    }
                    return true;
                }
                return false;
            }
            finally
            {
                sci.EndUndoAction();
            }
        }

         static public void Hide(char trigger)
        {

            if (completionList != null && isActive)
            {
                Hide();
                if (OnCancel != null)
                {
                    ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
                    if (!doc.IsEditable) return;
                    OnCancel(doc.SciControl, currentPos, currentWord, trigger, null);
                }
            }
        }

         static public void Hide()
        {

            if (completionList != null && isActive) 
            {
            //    tempo.Enabled = false;
                isActive = false;
                fullList = false;
     //           faded = false;
                completionList.Visible = false;
                if (completionList.Items.Count > 0) completionList.Items.Clear();
                currentItem = null;
                allItems = null;
                UITools.Tip.Hide();
                if (!UITools.CallTip.CallTipActive) UITools.Manager.UnlockControl();
            }
        }

         static public int IsAbbreviation(string label, string word)
        {
            int len = word.Length;
            int i = 1;
            char c = word[0];
            int p;
            int p2;
            int score = 0;
            if (label[0] == c) { p2 = 0; score = 1; }
            else if (label.IndexOf('.') < 0)
            {
                p2 = label.IndexOf(c);
                if (p2 < 0) return 0;
                score = 3;
            }
            else 
            {
                p2 = label.IndexOfOrdinal("." + c);
                if (p2 >= 0) { score = 2; p2++; }
                else
                {
                    p2 = label.IndexOf(c);
                    if (p2 < 0) return 0;
                    score = 4;
                }
            }
            int dist = 0;

            while (i < len)
            {
                p = p2;
                c = word[i++];
                if (Char.IsUpper(c)) p2 = label.IndexOfOrdinal(c.ToString(), p + 1);
                else p2 = label.IndexOf(c.ToString(), p + 1, StringComparison.OrdinalIgnoreCase);
                if (p2 < 0) return 0;

                int ups = 0; 
                for (int i2 = p + 1; i2 < p2; i2++) 
                    if (label[i2] == '_') { ups = 0; }
                    else if (Char.IsUpper(label[i2])) ups++;
                score += Math.Min(3, ups); // malus if skipped upper chars

                dist += p2 - p;
            }
            if (dist == len - 1)
            {
                if (label == word || label.EndsWithOrdinal("." + word)) return 1;
                return score;
            }
            else return score + 2;
        }



        static public string fItemName(ICompletionListItem item) {
            string _sLabel = item.Label;
                 int _nDot = _sLabel.IndexOf('.'); if(_nDot != -1) { _sLabel = _sLabel.Substring(_nDot + 1); }
                 return _sLabel;
        }

          static public int SmartMatch(string label, string word, int len)
        {
            if (label.Length < len) return 0;
            int _nDot = label.IndexOf('.'); if(_nDot != -1) { label = label.Substring(_nDot + 1); }
        //    TraceManager.Add("SmartMatch " + label  );
          //  TraceManager.Add("word " + word  );

            // simple matching
            if (disableSmartMatch)
            {
                if (label.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                {
                    if (label.StartsWithOrdinal(word)) return 1;
                    else return 5;
                }
                return 0;
            }

            // try abbreviation
            bool firstUpper = Char.IsUpper(word[0]);
            if (firstUpper)
            {
                int abbr = IsAbbreviation(label, word);
                if (abbr > 0) return abbr;
            }
                   
            int p = label.IndexOf(word, StringComparison.OrdinalIgnoreCase);
            if (p >= 0)
            {
                int p2;
                if (firstUpper) // try case sensitive search
                {
                    p2 = label.IndexOfOrdinal(word);
                    if (p2 >= 0)
                    {
                        int p3 = label.LastIndexOfOrdinal("." + word); // in qualified type name
                        if (p3 > 0)
                        {
                            if (p3 == label.LastIndexOf('.'))
                            {
                                if (label.EndsWithOrdinal("." + word)) return 1;
                                else return 3;
                            }
                            else return 4;
                        }
                    }
                    if (p2 == 0)
                    {
                        if (word == label) return 1;
                        else return 2;
                    }
                    else if (p2 > 0) return 4;
                }

                p2 = label.LastIndexOf("." + word, StringComparison.OrdinalIgnoreCase); // in qualified type name
                if (p2 > 0)
                {
                    if (p2 == label.LastIndexOf('.'))
                    {
                        if (label.EndsWith("." + word, StringComparison.OrdinalIgnoreCase)) return 2;
                        else return 4;
                    }
                    else return 5;
                }
                if (p == 0)
                {
                    if (label.Equals(word, StringComparison.OrdinalIgnoreCase))
                    {
                        if (label.Equals(word)) return 1;
                        else return 2;
                    }
                    else return 3;
                }
                else
                {
                    int p4 = label.IndexOf(':');
                    if (p4 > 0) return SmartMatch(label.Substring(p4 + 1), word, len);
                    return 5;
                }
            }

            // loose
            int firstChar = label.IndexOf(word[0].ToString(), StringComparison.OrdinalIgnoreCase);
            int i = 1;
            p = firstChar;
            while (i < len && p >= 0)
            {
                p = label.IndexOf(word[i++].ToString(), p + 1, StringComparison.OrdinalIgnoreCase);
            }
            return (p > 0) ? 7 : 0;
        }























 
        //NX+
        /// <summary>
        /// Shows the completion list
        /// </summary> 
        static public void ShowCL(List<ICompletionListItem> itemList, Boolean autoHide, String select)
        {

           // TraceManager.Add("SHOWW");
            if (!string.IsNullOrEmpty(select))
            {
                /*
                int maxLen = 0;
                foreach (ICompletionListItem item in itemList)
                    if (item.Label.Length > maxLen) maxLen = item.Label.Length;
                maxLen = Math.Min(256, maxLen);
                if (select.Length > maxLen) select = select.Substring(0, maxLen);*/
                currentWord = select;
            }
            else currentWord = null;


            ShowNx(itemList, autoHide);
        }

        //NX+
        /// <summary>
        /// Shows the completion list
        /// </summary>
        static public void ShowNx(List<ICompletionListItem> itemList, bool autoHide)
        {
            //TraceManager.Add("SHOWWW2");
            ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable) return;
            ScintillaControl sci = doc.SciControl;
            ListBox cl = completionList;
            try
            {
                if ((itemList == null) || (itemList.Count == 0))
                {
                    if (isActive) Hide();
                    return;
                }
                if (sci == null)
                {
                    if (isActive) Hide();
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ex);
            }
            // state
            allItems = itemList;
            autoHideList = autoHide;
            noAutoInsert = false;
            word = "";
            if (currentWord != null)
            {
                word = currentWord;
           //     currentWord = null;
            }
           


            MinWordLength = 1;
            fullList = (word.Length == 0) || !autoHide || !PluginBase.MainForm.Settings.AutoFilterList;
            lastIndex = 0;
            exactMatchInList = false;
            if (sci.SelectionStart == sci.SelectionEnd)
                startPos = sci.CurrentPos - word.Length;
            else
                startPos = sci.SelectionStart;


         //   TraceManager.Add("currentWord " + word);
          //  TraceManager.Add("startPos " + startPos);
            currentPos = sci.SelectionEnd; // sci.CurrentPos;
            defaultItem = null;
            // populate list
            needResize = true;

            tempo.Enabled = autoHide && (PluginBase.MainForm.Settings.DisplayDelay > 0);
            if (tempo.Enabled) tempo.Interval = PluginBase.MainForm.Settings.DisplayDelay;

            FindWordStartingWithNx(word);
            // state
            isActive = true;
            tempoTip.Enabled = false;
            showTime = DateTime.Now.Ticks;
            //disableSmartMatch = noAutoInsert || PluginBase.MainForm.Settings.DisableSmartMatch;
            disableSmartMatch = true; //NX+
            UITools.Manager.LockControl(sci);
            faded = false;
        }

        internal static void KeyDown()
        {
            if(isActive)
            {
                if(completionList.SelectedIndex < completionList.Items.Count)
                {
                      completionList.SelectedIndex++;
                }
            }
        }

        internal static void KeyUp()
        {
           if(isActive)
            {
                if(completionList.SelectedIndex > 0)
                {
                      completionList.SelectedIndex--;
                }
              

            }
        }

        private static int TestDefaultItem(Int32 index, String word, Int32 len)
        {
            if (defaultItem != null && completionList.Items.Contains(defaultItem))
            {
                Int32 score = (len == 0) ? 1 : SmartMatch(fItemName(defaultItem), word, len);
                if (score > 0 && score < 6) return completionList.Items.IndexOf(defaultItem);
            }
            return index;
        }

        /// <summary>
        /// Filter the completion list with the letter typed
        /// </summary> 
        static public void FindWordStartingWithNx(String word)
        {
            TraceManager.Add("FindWordStartingWith : " + word);
            if (word == null) word = "";
            Int32 len = word.Length;
            Int32 maxLen = 0;
            Int32 lastScore = 0;
            /// <summary>
            /// FILTER ITEMS
            /// </summary>
            List<ItemMatch> temp = new List<ItemMatch>(allItems.Count);


            List<ICompletionListItem> found;
            if (len == 0)
            {

                found = allItems;
                lastIndex = 0;
                exactMatchInList = false;
                smartMatchInList = true;
            }
            else
            {


                Int32 n = allItems.Count;
                Int32 i = 0;
                Int32 score;
                lastScore = 99;
                ICompletionListItem item;
                exactMatchInList = false;
                smartMatchInList = false;
                while (i < n)
                {
                    item = allItems[i];
                    // compare item's label with the searched word



                    score = SmartMatch(fItemName(item), word, len);
                    if (score > 0)
                    {
                        // first match found
                        if (!smartMatchInList || score < lastScore)
                        {
                            lastScore = score;
                            lastIndex = temp.Count;
                            smartMatchInList = true;
                            exactMatchInList = score < 5 && word == word;
                        }
                        temp.Add(new ItemMatch(score, item));
                        if (fItemName(item).Length > maxLen)
                        {
                            widestLabel = fItemName(item);
                            maxLen = widestLabel.Length;
                        }
                    }
                    else if (fullList) temp.Add(new ItemMatch(0, item));


                    i++;
                }
                // filter
                found = new List<ICompletionListItem>(temp.Count);
                if (word[0] == '_' && word.Length == 1)
                {
                    //All local var
                    for (int j = 0; j < temp.Count; j++)
                    {
                        if (j == lastIndex) lastIndex = found.Count;
                        char _sFirst = fItemName(temp[j].Item)[0];
                        if (_sFirst == '_') //local also
                        {
                            found.Add(temp[j].Item);
                        }
                    }

                    //List in priority same type
                }
                else if (word[0] == '_')
                {
                    ////////  local  ///////////
                    for (int j = 0; j < temp.Count; j++)
                    {

                        if (j == lastIndex) lastIndex = found.Count;
                        char _sFirst = fItemName(temp[j].Item)[0];
                        char _sSecond = fItemName(temp[j].Item)[1];
                        if (_sFirst == '_') //local also
                        {
                            if (_sSecond == word[1])
                            {
                                found.Add(temp[j].Item);
                            }
                        }

                    }

                }
                else
                {
                    //////// All Global //////////
                    for (int j = 0; j < temp.Count; j++)
                    {
                        if (j == lastIndex) lastIndex = found.Count;
                        if (fItemName(temp[j].Item)[0] == word[0])
                        {
                            found.Add(temp[j].Item);
                        }
                    }
                }

            }

            // no match?
            if (!smartMatchInList)
            {

                // smart match
                if (word.Length > 0)
                {
                    FindWordStartingWithNx(word.Substring(0, len - 1));
                }

                return;
            }
            if (found.Count == 0) //Nothing found / Show all global or local
            {
                //All local
                if (word[0] == '_')
                {
                    //All local var
                    for (int j = 0; j < temp.Count; j++)
                    {
                        if (j == lastIndex) lastIndex = found.Count;
                        char _sFirst = fItemName(temp[j].Item)[0];
                        if (_sFirst == '_') //local also
                        {
                            found.Add(temp[j].Item);
                        }
                    }
                }
                else
                {
                    //////// All Global //////////
                    for (int j = 0; j < temp.Count; j++)
                    {
                        if (j == lastIndex) lastIndex = found.Count;
                        if (fItemName(temp[j].Item)[0] == word[0])
                        {
                            found.Add(temp[j].Item);
                        }
                    }
                }

                return;
            }



            fullList = false;
  
            // is update needed?
            if ( completionList.Items.Count == found.Count)
            {
                int n =  completionList.Items.Count;
                bool changed = false;
                for (int i = 0; i < n; i++)
                {
                    if ( completionList.Items[i] != found[i])
                    {
                        changed = true;
                        break;
                    }
                }
                if (!changed)
                {
                    // preselected item
                    if (defaultItem != null)
                    {
                        if (lastScore > 3 || (lastScore > 2 && fItemName(defaultItem).StartsWith(word, StringComparison.OrdinalIgnoreCase)))
                        {
                            lastIndex = lastIndex = TestDefaultItem(lastIndex, word, len);
                        }
                    }
                     completionList.SelectedIndex = lastIndex;
                    return;
                }
            }
            // update
            try
            {
                 completionList.BeginUpdate();
                 completionList.Items.Clear();
                foreach (ICompletionListItem item in found)
                {
                     completionList.Items.Add(item);
                    if (item.Label.Length > maxLen)
                    {
                        widestLabel = item.Label;
                        maxLen = widestLabel.Length;
                    }
                }
                Int32 topIndex = lastIndex;
                if (defaultItem != null)
                {
                    if (lastScore > 3 || (lastScore > 2 && fItemName(defaultItem).StartsWith(word, StringComparison.OrdinalIgnoreCase)))
                    {
                        lastIndex = TestDefaultItem(lastIndex, word, len);
                    }
                }
                // select first item
                 completionList.TopIndex = topIndex;
                //  if (lastIndex != null)
                // {
                 completionList.SelectedIndex = lastIndex;
                //  }
            }
            catch (Exception ex)
            {
                Hide('\0');

                return;
            }
            finally
            {
                 completionList.EndUpdate();
            }
            // update list
             DisplayList(null, null);

        }
         static private void DisplayList(Object sender, System.Timers.ElapsedEventArgs e)
        {
            ITabbedDocument doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable) return;
            ScintillaControl sci = doc.SciControl;
            ListBox cl = completionList;
            if (cl.Items.Count == 0) return;

            // measure control
            if (needResize && !string.IsNullOrEmpty(widestLabel))
            {
                needResize = false;
                Graphics g = cl.CreateGraphics();
                SizeF size = g.MeasureString(widestLabel, cl.Font);
                cl.Width = (int)Math.Min(Math.Max(size.Width + 40, 100), ScaleHelper.Scale(400)) + ScaleHelper.Scale(10);
            }
            int newHeight = Math.Min(cl.Items.Count, 10) * cl.ItemHeight + 4;
            if (newHeight != cl.Height) cl.Height = newHeight;
            // place control
            Point coord = new Point(sci.PointXFromPosition(sci.CurrentPos), sci.PointYFromPosition(sci.CurrentPos));
            listUp = UITools.CallTip.CallTipActive || (coord.Y+cl.Height > sci.Height);
            coord = sci.PointToScreen(coord);
            coord = ((Form)PluginBase.MainForm).PointToClient(coord);
            cl.Left = coord.X-20 + sci.Left;
            if (listUp) cl.Top = coord.Y-cl.Height;
            else cl.Top = coord.Y + UITools.Manager.LineHeight(sci);
            // Keep on control area
            if (cl.Right > ((Form)PluginBase.MainForm).ClientRectangle.Right)
            {
                cl.Left = ((Form)PluginBase.MainForm).ClientRectangle.Right - cl.Width;
            }
            if (!cl.Visible)
            {
                Redraw();
                cl.Show();
                cl.BringToFront();
                if (UITools.CallTip.CallTipActive) UITools.CallTip.PositionControl(sci);
            }
        }
            static public void Redraw()
        {
            Color back = PluginBase.MainForm.GetThemeColor("CompletionList.BackColor");
            completionList.BackColor = back == Color.Empty ? System.Drawing.SystemColors.Window : back;
        }

        
    }
}
