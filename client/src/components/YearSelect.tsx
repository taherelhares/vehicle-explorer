import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import { monoFamily } from '../theme';
import { FilterField, plainControlSx } from './FilterField';

interface Props {
  value: number;
  onChange: (year: number) => void;
}

// The API deliberately does not range-check the year - vPIC answers an implausible one
// with an empty list rather than an error. Offering a fixed range here is a convenience
// for the person using the app, not a rule the server relies on.
const latest = new Date().getFullYear() + 1;
const earliest = 1980;
const years = Array.from({ length: latest - earliest }, (_, i) => latest - i);

export function YearSelect({ value, onChange }: Props) {
  return (
    <FilterField
      label="Model year"
      labelId="year-label"
      helper={`${earliest + 1}-${latest}`}
    >
      <Select
        labelId="year-label"
        variant="standard"
        fullWidth
        value={value}
        onChange={(event) => onChange(Number(event.target.value))}
        sx={{ ...plainControlSx, fontFamily: monoFamily }}
        MenuProps={{ sx: { '& .MuiMenu-paper': { maxHeight: 320 } } }}
      >
        {years.map((year) => (
          <MenuItem key={year} value={year} sx={{ fontFamily: monoFamily }}>
            {year}
          </MenuItem>
        ))}
      </Select>
    </FilterField>
  );
}
