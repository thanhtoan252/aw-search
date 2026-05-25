import { useState, useEffect, useRef } from "react";

interface Props {
  value: string;
  onChange: (value: string) => void;
}

export function SearchBar({ value, onChange }: Props) {
  const [local, setLocal] = useState(value);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }
    timerRef.current = setTimeout(() => onChange(local), 350);
    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
    };
  }, [local]);

  return (
    <div className="search-bar">
      <span className="search-icon">⌕</span>
      <input
        type="text"
        placeholder="Search products... (e.g. Mountain Bike, Road Frame, Helmet)"
        value={local}
        onChange={(e) => setLocal(e.target.value)}
        className="search-input"
        autoFocus
      />
      {local && (
        <button
          className="search-clear"
          onClick={() => { setLocal(""); onChange(""); }}
          aria-label="Clear"
        >
          ✕
        </button>
      )}
    </div>
  );
}
