import React, { useEffect, useState } from 'react'
import { TicketBadge } from '../components/TicketBadge'

type Ticket = { id: string, eventId: string, status: string, purchasedAt: string }

export function DashboardView() {
  const [tickets, setTickets] = useState<Ticket[]>([])
  async function load() {
    const res = await fetch('/Dashboard/Index?handler=Tickets')
    setTickets(await res.json())
  }
  async function cancel(id: string) {
    const form = new FormData(); form.append('ticketId', id)
    const res = await fetch('/Dashboard/Index?handler=Cancel', { method: 'POST', body: form })
    if (res.ok) load()
  }
  useEffect(() => { load() }, [])
  return (
    <div>
      <h3 className="mb-3">My Tickets</h3>
      {tickets.length === 0 && <div className="alert alert-secondary">No tickets yet.</div>}
      <ul className="list-group">
        {tickets.map(t => (
          <li key={t.id} className="list-group-item d-flex justify-content-between align-items-center">
            <div>
              <TicketBadge status={t.status} />
              <span className="ms-2 small text-muted">Purchased {new Date(t.purchasedAt).toLocaleString()}</span>
            </div>
            {t.status !== 'Cancelled' && <button className="btn btn-outline-danger btn-sm" onClick={() => cancel(t.id)}>Cancel</button>}
          </li>
        ))}
      </ul>
    </div>
  )
}


