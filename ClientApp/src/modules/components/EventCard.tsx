import React from 'react'

export function EventCard({ title, dateTime, venue, category, remaining, onOpen }: {
  title: string
  dateTime: string
  venue: string
  category: string
  remaining: number
  onOpen?: () => void
}) {
  return (
    <div className="card mb-3">
      <div className="card-body d-flex justify-content-between align-items-start gap-3">
        <div>
          <div className="mb-1">
            <span className="badge text-bg-secondary me-2">{category}</span>
            <h5 className="card-title d-inline">{title}</h5>
          </div>
          <div className="text-muted small">{new Date(dateTime).toLocaleString()} · {venue}</div>
          <div className="mt-2">
            <span className="badge text-bg-info">Remaining: {remaining}</span>
          </div>
        </div>
        {onOpen && (
          <div>
            <button className="btn btn-primary" onClick={onOpen}>View details</button>
          </div>
        )}
      </div>
    </div>
  )
}


