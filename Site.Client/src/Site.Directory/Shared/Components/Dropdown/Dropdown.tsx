import React, { useState, useRef, useEffect } from 'react';
import './Dropdown.scss';

interface DropdownOption {
  value: string;
  label: string;
}

interface DropdownProps {
  name: string;
  options: DropdownOption[];
  placeholder?: string;
  onSelect?: (value: string) => void;
}

const Dropdown: React.FC<DropdownProps> = ({ name, options, placeholder = 'Select an option', onSelect }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedValue, setSelectedValue] = useState<string | null>(null);
  const [selectedLabel, setSelectedLabel] = useState<string>(placeholder);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const handleSelect = (option: DropdownOption) => {
    setSelectedValue(option.value);
    setSelectedLabel(option.label);
    setIsOpen(false);
    if (onSelect) {
      onSelect(option.value);
    }
  };

  const handleClickOutside = (event: MouseEvent) => {
    if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
      setIsOpen(false);
    }
  };

  useEffect(() => {
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  return (
    <div
      ref={dropdownRef}
      className={`rpg-dropdown ${isOpen ? 'active' : ''}`}
      onClick={() => setIsOpen(!isOpen)}
      tabIndex={0}
    >
      <div className="select">
        <span>{selectedLabel}</span>
        <i className="fa fa-chevron-left"></i>
      </div>
      <input type="hidden" name={name} value={selectedValue || ''} />
      {isOpen && (
        <ul className="rpg-dropdown-menu" style={{ display: 'block' }}>
          {options.map((option) => (
            <li
              key={option.value}
              id={option.value}
              onClick={() => handleSelect(option)}
            >
              {option.label}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default Dropdown;