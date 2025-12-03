import { Check, X } from "lucide-react";

interface ValidationRule {
  label: string;
  valid: boolean;
}

interface ValidationHintProps {
  rules: ValidationRule[];
  show: boolean;
}

export function ValidationHint({ rules, show }: ValidationHintProps) {
  if (!show) return null;

  return (
    <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mt-2">
      <p className="text-xs font-medium text-blue-900 mb-2">Requisitos:</p>
      <ul className="space-y-1">
        {rules.map((rule, index) => (
          <li key={index} className="flex items-center gap-2 text-xs">
            {rule.valid ? (
              <Check className="h-3 w-3 text-green-600" />
            ) : (
              <X className="h-3 w-3 text-red-600" />
            )}
            <span className={rule.valid ? 'text-green-700' : 'text-gray-700'}>
              {rule.label}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
