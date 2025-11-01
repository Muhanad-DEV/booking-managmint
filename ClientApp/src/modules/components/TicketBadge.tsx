import React from 'react'

export function TicketBadge({ status }: { status: string }) {
  const color = status === 'Paid' ? 'success' : status === 'Cancelled' ? 'danger' : status === 'CheckedIn' ? 'primary' : 'secondary'
  return <span className={`badge text-bg-${color}`}>{status}</span>
}


