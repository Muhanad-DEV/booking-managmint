import React, { useEffect, useState } from 'react'
import { EventCard } from './components/EventCard'
import { Filters } from './components/Filters'

type EventItem = { id: string, title: string, dateTime: string, venue: string, category: string, remaining: number }

export function App() {
  const [query, setQuery] = useState('')
  const [featured, setFeatured] = useState<EventItem[]>([])
  const [items, setItems] = useState<EventItem[]>([])
  const [visible, setVisible] = useState(5)
  const [loading, setLoading] = useState(false)

  async function loadFeatured() {
    const res = await fetch('/Events?handler=List&q=')
    const all = await res.json()
    setFeatured(all.slice(0, 3))
  }

  async function loadList(q: string) {
    setLoading(true)
    const res = await fetch(`/Events?handler=List&q=${encodeURIComponent(q)}`)
    const data = await res.json()
    setItems(data)
    setVisible(5)
    setLoading(false)
  }

  useEffect(() => { loadFeatured(); loadList('') }, [])

  function applySearch() {
    loadList(query)
  }

  function quick(q: string) {
    setQuery(q); loadList(q)
  }

  return (
    <div className="container">
      <div className="py-4">
        <h2 className="mb-3">Browse Events</h2>
        <div className="input-group input-group-lg mb-3" style={{maxWidth: 640}}>
          <span className="input-group-text">Search</span>
          <input
            className="form-control"
            placeholder="event title, venue, or category"
            value={query}
            onChange={e => setQuery(e.target.value)}
            onKeyDown={e => { if (e.key === 'Enter') applySearch() }}
          />
          <button className="btn btn-primary" onClick={applySearch}>Search</button>
        </div>
        <div className="d-flex gap-2 mb-4">
          <button className="btn btn-outline-secondary btn-sm" onClick={() => quick('Tech')}>Tech</button>
          <button className="btn btn-outline-secondary btn-sm" onClick={() => quick('Workshop')}>Workshops</button>
          <button className="btn btn-outline-secondary btn-sm" onClick={() => quick('Auditorium')}>On Campus</button>
        </div>

        <div className="row g-3">
          <div className="col-12 col-md-4">
            <div className="card">
              <div className="card-body">
                <h5 className="card-title">Filters</h5>
                <Filters value={query} onChange={setQuery} />
                <button className="btn btn-primary w-100" onClick={applySearch} disabled={loading}>Apply</button>
              </div>
            </div>
          </div>
          <div className="col-12 col-md-8">
            <h5 className="mb-3">Featured</h5>
            {featured.length === 0 && <div className="alert alert-secondary">No upcoming events yet.</div>}
            {featured.map(e => (
              <EventCard key={e.id} title={e.title} dateTime={e.dateTime} venue={e.venue} category={e.category} remaining={e.remaining}
                onOpen={() => { window.location.href = `/Events/Details/${e.id}` }} />
            ))}
            <h5 className="mt-4 mb-3">All events</h5>
            {loading && <div className="alert alert-info">Loading…</div>}
            {!loading && items.slice(0, visible).map(e => (
              <EventCard key={e.id} title={e.title} dateTime={e.dateTime} venue={e.venue} category={e.category} remaining={e.remaining}
                onOpen={() => { window.location.href = `/Events/Details/${e.id}` }} />
            ))}
            {!loading && items.length === 0 && <div className="alert alert-warning">No events found.</div>}
            {!loading && visible < items.length && (
              <button className="btn btn-outline-secondary" onClick={() => setVisible(v => v + 5)}>Load more</button>
            )}
            <div className="mt-2">
              <a className="btn btn-link p-0" href="/Events">Open full events page →</a>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

