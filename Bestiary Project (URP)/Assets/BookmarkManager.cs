using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookmarkManager : MonoBehaviour
{
    public List<Bookmark> bookmarks = new List<Bookmark>();
    private void OnEnable()
    {
        Book.ChapterChanged += BookmarkUpdate;
    }
    private void OnDisable()
    {
        Book.ChapterChanged -= BookmarkUpdate;
    }
    private void BookmarkUpdate()
    {
        int chapter = (int)Book.instance.currentChapter -1;
        for (int i = 0; i < bookmarks.Count; i++)
        {
            bookmarks[i].active = i == chapter;
            bookmarks[i].onLeft = i < chapter;
        }
    }
}
