using System.Collections.Generic;
using System.Linq;

public class NoteService
{
    readonly List<NoteRecord> _notes;
    readonly SaveService _save;

    public NoteService(SaveService save)
    {
        _save = save;
        _notes = save.Data.notes;
    }

    public void AddNote(int week, string text, string source)
    {
        // Don't duplicate
        if (_notes.Any(n => n.week == week && n.text == text))
            return;
        _notes.Add(new NoteRecord { week = week, text = text, source = source });
        _save.Save();
    }

    public void RemoveNote(int week, string text)
    {
        _notes.RemoveAll(n => n.week == week && n.text == text);
        _save.Save();
    }

    public bool HasNote(int week, string text) =>
        _notes.Any(n => n.week == week && n.text == text);

    public List<NoteRecord> GetNotes(int week) =>
        _notes.Where(n => n.week == week).ToList();

    public int NoteCount(int week) =>
        _notes.Count(n => n.week == week);
}
