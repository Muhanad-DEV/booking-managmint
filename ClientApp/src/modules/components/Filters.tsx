import React from 'react'

export function Filters({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  return (
    <div className="input-group mb-3">
      <span className="input-group-text">Search</span>
      <input className="form-control" value={value} onChange={e => onChange(e.target.value)} placeholder="event title" />
    </div>
  )
}


