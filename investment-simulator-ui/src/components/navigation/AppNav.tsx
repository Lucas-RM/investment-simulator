import { NavLink } from 'react-router-dom';
import { paths } from '@/routes/paths';
import styles from './AppNav.module.css';

const links = [
  { to: paths.home, label: 'Início', end: true },
  { to: paths.cdb, label: 'CDB', end: false },
  { to: paths.tesouro, label: 'Tesouro Selic', end: false },
  { to: paths.compare, label: 'Comparar', end: false },
  { to: paths.history, label: 'Histórico', end: false },
] as const;

export function AppNav() {
  return (
    <nav className={styles.nav} aria-label="Principal">
      {links.map((link) => (
        <NavLink
          key={link.to}
          to={link.to}
          end={link.end}
          className={({ isActive }) =>
            isActive ? `${styles.link} ${styles.active}` : styles.link
          }
        >
          {link.label}
        </NavLink>
      ))}
    </nav>
  );
}
