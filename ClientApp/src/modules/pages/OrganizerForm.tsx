import React, { useState } from 'react'

export function OrganizerForm() {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [dateTime, setDateTime] = useState('')
  const [venue, setVenue] = useState('')
  const [capacity, setCapacity] = useState(20)
  const [price, setPrice] = useState(0)
  const [category, setCategory] = useState('General')
  const [msg, setMsg] = useState('')

  async function submit() {
    const form = new FormData()
    form.append('title', title)
    form.append('description', description)
    form.append('dateTime', dateTime)
    form.append('venue', venue)
    form.append('capacity', String(capacity))
    form.append('price', String(price))
    form.append('category', category)
    const res = await fetch('/Organizer/Events?handler=Create', { method: 'POST', body: form })
    if (res.ok) setMsg('Event created!')
    else setMsg('Failed to create event')
  }

  return (
    <div className="card">
      <div className="card-body">
        <h3 className="card-title">Create Event</h3>
        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label">Title</label>
            <input className="form-control" value={title} onChange={e => setTitle(e.target.value)} required maxLength={120} />
          </div>
          <div className="col-md-6">
            <label className="form-label">Venue</label>
            <input className="form-control" value={venue} onChange={e => setVenue(e.target.value)} required />
          </div>
          <div className="col-12">
            <label className="form-label">Description</label>
            <textarea className="form-control" value={description} onChange={e => setDescription(e.target.value)} />
          </div>
          <div className="col-md-4">
            <label className="form-label">Date & Time</label>
            <input type="datetime-local" className="form-control" value={dateTime} onChange={e => setDateTime(e.target.value)} required />
          </div>
          <div className="col-md-4">
            <label className="form-label">Capacity</label>
            <input type="number" className="form-control" min={0} value={capacity} onChange={e => setCapacity(parseInt(e.target.value||'0'))} />
          </div>
          <div className="col-md-4">
            <label className="form-label">Price</label>
            <input type="number" className="form-control" min={0} step={0.01} value={price} onChange={e => setPrice(parseFloat(e.target.value||'0'))} />
          </div>
          <div className="col-md-6">
            <label className="form-label">Category</label>
            <input className="form-control" value={category} onChange={e => setCategory(e.target.value)} />
          </div>
        </div>
        <button className="btn btn-primary mt-3" onClick={submit}>Create</button>
        {msg && <div className="alert alert-info mt-3">{msg}</div>}
      </div>
    </div>
  )
}


