using System;

/// <summary>
/// Stores the initial DateTime value and allows to get time elapsed since the start
/// </summary>
public class DateTimeTimer {

    private DateTime m_startTimeValue;
    
    public DateTimeTimer()
    {
        m_startTimeValue = new DateTime();
    }

    public DateTimeTimer Start()
    {
        m_startTimeValue = DateTime.Now;
        return this;
    }

    public DateTime GetStartTime()
    {
        return m_startTimeValue;
    }

    public TimeSpan GetElapsedTime()
    {
        return DateTime.Now - m_startTimeValue;
    }

}