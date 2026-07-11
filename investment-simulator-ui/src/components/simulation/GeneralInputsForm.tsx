import { useId, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import type { GeneralInputs } from '@/types/generalInputs'
import {
  INVESTMENT_TYPE_LABELS,
  InvestmentType,
} from '@/types/investment'
import { paths } from '@/routes/paths'
import {
  hasGeneralInputsErrors,
  validateGeneralInputs,
} from '@/utils/validateGeneralInputs'
import styles from './GeneralInputsForm.module.css'

export type GeneralInputsFormProps = {
  /** Pre-selected investment type (usually from the current route). */
  defaultInvestmentType: InvestmentType
  /** Optional initial values for amount and dates. */
  defaultValues?: Partial<
    Pick<GeneralInputs, 'initialAmount' | 'startDate' | 'endDate'>
  >
  /**
   * Called when the form passes client-side validation.
   * Submit / API wiring is left to later commits.
   */
  onValidSubmit?: (values: GeneralInputs) => void
}

function pathForType(type: InvestmentType): string {
  return type === InvestmentType.Cdb ? paths.cdb : paths.tesouro
}

export function GeneralInputsForm({
  defaultInvestmentType,
  defaultValues,
  onValidSubmit,
}: GeneralInputsFormProps) {
  const navigate = useNavigate()
  const formId = useId()

  const [values, setValues] = useState<GeneralInputs>({
    investmentType: defaultInvestmentType,
    initialAmount: defaultValues?.initialAmount ?? '0',
    startDate: defaultValues?.startDate ?? '',
    endDate: defaultValues?.endDate ?? '',
  })
  const [errors, setErrors] = useState<
    Partial<Record<keyof GeneralInputs, string>>
  >({})
  const [submitted, setSubmitted] = useState(false)

  function updateField<K extends keyof GeneralInputs>(
    field: K,
    value: GeneralInputs[K],
  ) {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => {
      if (!current[field]) {
        return current
      }
      const next = { ...current }
      delete next[field]
      return next
    })
    setSubmitted(false)
  }

  function handleInvestmentTypeChange(nextType: InvestmentType) {
    updateField('investmentType', nextType)
    if (nextType !== defaultInvestmentType) {
      navigate(pathForType(nextType))
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const nextErrors = validateGeneralInputs(values)
    setErrors(nextErrors)

    if (hasGeneralInputsErrors(nextErrors)) {
      setSubmitted(false)
      return
    }

    setSubmitted(true)
    onValidSubmit?.(values)
  }

  const amountId = `${formId}-amount`
  const startId = `${formId}-start`
  const endId = `${formId}-end`
  const typeId = `${formId}-type`

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <fieldset className={styles.fieldset}>
        <legend className={styles.legend}>Entradas gerais</legend>
        <p className={styles.hint}>
          Informe o valor inicial, o período da simulação e o tipo de
          investimento.
        </p>

        <div className={styles.field}>
          <label htmlFor={typeId}>Tipo de investimento</label>
          <select
            id={typeId}
            name="investmentType"
            value={values.investmentType}
            onChange={(event) =>
              handleInvestmentTypeChange(
                event.target.value as InvestmentType,
              )
            }
            aria-invalid={Boolean(errors.investmentType)}
            aria-describedby={
              errors.investmentType ? `${typeId}-error` : undefined
            }
          >
            <option value={InvestmentType.Cdb}>
              {INVESTMENT_TYPE_LABELS[InvestmentType.Cdb]}
            </option>
            <option value={InvestmentType.TesouroSelic}>
              {INVESTMENT_TYPE_LABELS[InvestmentType.TesouroSelic]}
            </option>
          </select>
          {errors.investmentType ? (
            <p id={`${typeId}-error`} className={styles.error} role="alert">
              {errors.investmentType}
            </p>
          ) : null}
        </div>

        <div className={styles.field}>
          <label htmlFor={amountId}>Valor inicial (R$)</label>
          <input
            id={amountId}
            name="initialAmount"
            type="text"
            inputMode="decimal"
            autoComplete="off"
            placeholder="0.00"
            value={values.initialAmount}
            onChange={(event) =>
              updateField('initialAmount', event.target.value)
            }
            aria-invalid={Boolean(errors.initialAmount)}
            aria-describedby={
              errors.initialAmount
                ? `${amountId}-error ${amountId}-hint`
                : `${amountId}-hint`
            }
          />
          <p id={`${amountId}-hint`} className={styles.fieldHint}>
            Pode ser zero se houver aportes adicionais depois.
          </p>
          {errors.initialAmount ? (
            <p
              id={`${amountId}-error`}
              className={styles.error}
              role="alert"
            >
              {errors.initialAmount}
            </p>
          ) : null}
        </div>

        <div className={styles.row}>
          <div className={styles.field}>
            <label htmlFor={startId}>Data inicial</label>
            <input
              id={startId}
              name="startDate"
              type="date"
              value={values.startDate}
              onChange={(event) =>
                updateField('startDate', event.target.value)
              }
              aria-invalid={Boolean(errors.startDate)}
              aria-describedby={
                errors.startDate ? `${startId}-error` : undefined
              }
            />
            {errors.startDate ? (
              <p
                id={`${startId}-error`}
                className={styles.error}
                role="alert"
              >
                {errors.startDate}
              </p>
            ) : null}
          </div>

          <div className={styles.field}>
            <label htmlFor={endId}>Data de resgate</label>
            <input
              id={endId}
              name="endDate"
              type="date"
              value={values.endDate}
              onChange={(event) =>
                updateField('endDate', event.target.value)
              }
              aria-invalid={Boolean(errors.endDate)}
              aria-describedby={
                errors.endDate ? `${endId}-error` : undefined
              }
            />
            {errors.endDate ? (
              <p id={`${endId}-error`} className={styles.error} role="alert">
                {errors.endDate}
              </p>
            ) : null}
          </div>
        </div>
      </fieldset>

      <div className={styles.actions}>
        <button type="submit" className={styles.submit}>
          Continuar
        </button>
        {submitted ? (
          <p className={styles.success} role="status">
            Entradas gerais válidas. Próximos passos (aportes e taxas)
            serão adicionados em seguida.
          </p>
        ) : null}
      </div>
    </form>
  )
}
