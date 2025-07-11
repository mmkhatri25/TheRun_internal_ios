using System;
using System.Collections.Generic;
using UnityEngine;

public class SRTParser
{
    public List<SubtitleBlock> _subtitles;

    int count = 0;

    public SRTParser(string textAssetResourcePath)
    {
        var text = Resources.Load<TextAsset>(textAssetResourcePath);
        Load(text);
    }

    public SRTParser(TextAsset textAsset)
    {
        this._subtitles = Load(textAsset);
    }

    static public List<SubtitleBlock> Load(TextAsset textAsset)
    {
        if (textAsset == null)
        {
            Debug.LogError("Subtitle file is null");
            return null;
        }

        var lines = textAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        var currentState = eReadState.Index;

        var subs = new List<SubtitleBlock>();

        int currentIndex = 0;
        double currentFrom = 0, currentTo = 0;
        var currentText = string.Empty;
        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];

            switch (currentState)
            {
                case eReadState.Index:
                    {
                        int index;
                        if (Int32.TryParse(line, out index))
                        {
                            currentIndex = index;
                            currentState = eReadState.Time;
                        }
                    }
                    break;
                case eReadState.Time:
                    {
                        line = line.Replace(',', '.');
                        var parts = line.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);

                        // Parse the timestamps
                        if (parts.Length == 2)
                        {
                            TimeSpan fromTime;
                            if (TimeSpan.TryParse(parts[0], out fromTime))
                            {
                                TimeSpan toTime;
                                if (TimeSpan.TryParse(parts[1], out toTime))
                                {
                                    currentFrom = fromTime.TotalSeconds;
                                    currentTo = toTime.TotalSeconds;
                                    currentState = eReadState.Text;
                                }
                            }
                        }
                    }
                    break;
                case eReadState.Text:
                    {
                        if (currentText != string.Empty)
                            currentText += "\r\n";

                        currentText += line;

                        // When we hit an empty line, consider it the end of the text
                        if (string.IsNullOrEmpty(line) || l == lines.Length - 1)
                        {
                            // Create the SubtitleBlock with the data we've aquired 
                            subs.Add(new SubtitleBlock(currentIndex, currentFrom, currentTo, currentText));

                            // Reset stuff so we can start again for the next block
                            currentText = string.Empty;
                            currentState = eReadState.Index;
                        }
                    }
                    break;
            }
        }
        return subs;
    }

    public int GetSubtitlesCount()
    {
        return count;
    }

    public void RemoveAlreadyReadSubtitles(int count)
    {
        //_subtitles.RemoveRange(0, count);

        if (_subtitles.Count == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            if (_subtitles.Count == 0)
                return;

            _subtitles.RemoveAt(0);
        }
    }

    public SubtitleBlock GetForTime(float time)
    {
        if (_subtitles.Count > 0)
        {
            var subtitle = _subtitles[0];

            if (time >= subtitle.To)
            {
                _subtitles.RemoveAt(0);
                count++;

                //Debug.Log("Subtitles Removed ---- " + count);

                if (_subtitles.Count == 0)
                    return null;


                subtitle = _subtitles[0];

                // Debug.Log("------------" + subtitle.Text);
            }

            if (subtitle.From > time)
            {
                // Debug.Log("Return From Blank");
                return SubtitleBlock.Blank;
            }

            //Debug.Log("Return From TextSubtitle");

            return subtitle;
        }
        return null;
    }

    public void RemoveSubtitlesOnSkip(float time)
    {
        if (_subtitles.Count > 0)
        {
            int count = _subtitles.Count;

            for (int i = 0; i < count; i++)
            {
                var subtitle = _subtitles[0];

                //Debug.Log("Time Testing Time== " + time + "  TO time == " + subtitle.To + " Count == " + _subtitles.Count);

                if (time > subtitle.To)
                    _subtitles.RemoveAt(0);
                else
                    return;

                if (_subtitles.Count == 0)
                    return;
            }

        }
        return;
    }

    enum eReadState
    {
        Index,
        Time,
        Text
    }
}

public class SubtitleBlock
{
    static SubtitleBlock _blank;
    public static SubtitleBlock Blank
    {
        get { return _blank ?? (_blank = new SubtitleBlock(0, 0, 0, string.Empty)); }
    }
    public int Index { get; private set; }
    public double Length { get; private set; }
    public double From { get; private set; }
    public double To { get; private set; }
    public string Text { get; private set; }

    public SubtitleBlock(int index, double from, double to, string text)
    {
        this.Index = index;
        this.From = from;
        this.To = to;
        this.Length = to - from;
        this.Text = text;
    }
}
