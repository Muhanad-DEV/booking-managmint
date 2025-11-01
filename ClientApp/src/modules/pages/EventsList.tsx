import React, { useEffect, useState } from 'react'
import { EventCard } from '../components/EventCard'
import { Filters } from '../components/Filters'

type EventItem = { id: string, title: string, dateTime: string, venue: string, category: string, remaining: number }

export function EventsList() {
  const [q, setQ] = useState('')
  const [items, setItems] = useState<EventItem[]>([])

  async function load() {
    const res = await fetch(`/Events?handler=List&q=${encodeURIComponent(q)}`)
    setItems(await res.json())
  }

  useEffect(() => { load() }, [q])

  return (
    <div>
      <Filters value={q} onChange={setQ} />
      {items.map(e => (
        <EventCard key={e.id} title={e.title} dateTime={e.dateTime} venue={e.venue} category={e.category} remaining={e.remaining}
          onOpen={() => { window.location.href = `/Events/Details/${e.id}` } } />
      ))}
      {items.length === 0 && <div className="alert alert-warning">No events found.</div>}
    </div>
  )
}


