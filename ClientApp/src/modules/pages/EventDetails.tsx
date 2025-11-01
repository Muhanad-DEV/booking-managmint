import React, { useEffect, useState } from 'react'

type EventDetails = { id: string, title: string, description: string, dateTime: string, venue: string, capacity: number, remaining: number, price: number, category: string }

export function EventDetails() {
  const id = window.location.pathname.split('/').pop() as string
  const [ev, setEv] = useState<EventDetails | null>(null)
  const [qty, setQty] = useState(1)
  const [msg, setMsg] = useState('')

  async function load() {
    const res = await fetch(`/Events/Details/${encodeURIComponent(id)}?handler=Details`)
    if (res.ok) setEv(await res.json())
  }
  async function book() {
    const form = new FormData()
    form.append('id', id)
    form.append('quantity', String(qty))
    const res = await fetch(`/Events/Details/${encodeURIComponent(id)}?handler=Book`, { method: 'POST', body: form })
    if (res.ok) {
      const x = await res.json(); setMsg(`Booked! Ticket #${x.ticketId}`)
      await load()
    } else {
      setMsg('Booking failed')
    }
  }

  useEffect(() => { load() }, [])

  if (!ev) return <div className="alert alert-info">Loading...</div>
  return (
    <div className="card">
      <div className="card-body">
        <h3 className="card-title">{ev.title}</h3>
        <div className="text-muted">{new Date(ev.dateTime).toLocaleString()} – {ev.venue} · <span className="badge text-bg-secondary">{ev.category}</span></div>
        <p className="mt-3">{ev.description}</p>
        <div className="mb-2"><span className="badge text-bg-info">Remaining: {ev.remaining}/{ev.capacity}</span></div>
        <div className="input-group" style={{maxWidth: 240}}>
          <span className="input-group-text">Qty</span>
          <input type="number" min={1} max={ev.remaining} value={qty} onChange={e => setQty(parseInt(e.target.value||'1'))} className="form-control"/>
          <button className="btn btn-primary" onClick={book} disabled={ev.remaining <= 0}>Book</button>
        </div>
        {msg && <div className="alert alert-success mt-3">{msg}</div>}
      </div>
    </div>
  )
}


