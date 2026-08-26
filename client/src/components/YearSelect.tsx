import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';

interface Props {
  value: number;
  onChange: (year: number) => void;
}

// The API deliberately does not range-check the year — vPIC answers an implausible one
// with an empty list rather than an error. Offering a fixed range here is a convenience
// for the person using the app, not a rule the server relies on.
const latest = new Date().getFullYear() + 1;
const years = Array.from({ length: latest - 1980 }, (_, i) => latest - i);

export function YearSelect({ value, onChange }: Props) {
  return (
    <FormControl fullWidth>
      <InputLabel id="year-label">Model year</InputLabel>
      <Select
        labelId="year-label"
        label="Model year"
        value={value}
        onChange={(event) => onChange(Number(event.target.value))}
      >
        {years.map((year) => (
          <MenuItem key={year} value={year}>
            {year}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
