import React from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './modules/App'
import { EventsList } from './modules/pages/EventsList'
import { EventDetails } from './modules/pages/EventDetails'
import { DashboardView } from './modules/pages/DashboardView'
import { OrganizerForm } from './modules/pages/OrganizerForm'

declare global {
  interface Window { __REACT_MOUNTS__?: Array<{ id: string, component: string }> }
}

function mount(component: string, el: HTMLElement) {
  const root = createRoot(el)
  switch (component) {
    case 'EventsList':
      root.render(<EventsList />)
      break
    case 'EventDetails':
      root.render(<EventDetails />)
      break
    case 'DashboardView':
      root.render(<DashboardView />)
      break
    case 'OrganizerForm':
      root.render(<OrganizerForm />)
      break
    case 'EventsHome':
    default:
      root.render(<App />)
  }
}

if (window.__REACT_MOUNTS__) {
  window.__REACT_MOUNTS__.forEach(({ id, component }) => {
    const el = document.getElementById(id)
    if (el) mount(component, el)
  })
}

