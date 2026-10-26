export const isCritical = (tc) => String(tc?.priority || '').toLowerCase() === 'critical'

// Result of one test case = the latest run of each assigned tester
export const caseResult = (testCaseId, runs) => {
  const mine = (runs || []).filter((r) => String(r.test_case_id) === String(testCaseId))
  if (!mine.length) return 'Not Run'

  const latestByTester = {}
  mine.slice().sort((a, b) => (a.id || 0) - (b.id || 0)).forEach((r) => { latestByTester[r.executed_by] = r })
  const latest = Object.values(latestByTester)

  if (latest.some((r) => ['Failed', 'Fail'].includes(r.run_status))) return 'Failed'
  if (latest.some((r) => r.run_status === 'Blocked')) return 'Blocked'
  if (latest.every((r) => r.run_status === 'Passed')) return 'Passed'
  return 'In Progress'
}

export const evaluateCycle = (cases, runs) => {
  const list = cases || []
  const results = {}
  list.forEach((c) => { results[c.id] = caseResult(c.id, runs) })

  const count = (arr, r) => arr.filter((c) => results[c.id] === r).length
  const critical = list.filter(isCritical)
  const total = list.length
  const passed = count(list, 'Passed')
  const failed = count(list, 'Failed')
  const cPassed = count(critical, 'Passed')
  const cFailed = count(critical, 'Failed')

  let verdict
  if (!total) verdict = 'No test cases'
  else if (critical.length) verdict = cFailed ? 'Failed' : cPassed === critical.length ? 'Passed' : 'Pending'
  else verdict = failed ? 'Failed' : passed === total ? 'Passed' : 'Pending'

  return {
    results,
    verdict,
    basis: critical.length ? 'critical' : 'all',
    total,
    passed,
    failed,
    pct: total ? Math.round((passed / total) * 100) : 0,
    critical: {
      total: critical.length,
      passed: cPassed,
      failed: cFailed,
      pending: critical.length - cPassed - cFailed,
    },
    nonCriticalFailed: failed - cFailed,
  }
}

export const verdictColor = (v) =>
  v === 'Passed' ? 'success' : v === 'Failed' ? 'error' : v === 'Pending' ? 'warning' : 'grey'

export const caseResultColor = (r) =>
  r === 'Passed' ? 'success' : r === 'Failed' ? 'error' : r === 'Blocked' ? 'orange' : r === 'In Progress' ? 'info' : 'grey'